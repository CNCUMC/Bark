<#
.SYNOPSIS
    构建模组并发布到 NexusMods 和/或 GitHub Release。

.DESCRIPTION
    1. 从 Plugin.cs 读取模组信息（Name, Version, GUID）
    2. 构建项目
    3. 收集文件并压缩为 {Name}-v{Version}.zip
    4. 上传到 NexusMods (通过 API)
    5. 上传到 GitHub Release (通过 gh CLI)

.PARAMETER Configuration
    构建配置 (Debug/Release)，默认 Release。

.PARAMETER NexusApiKey
    NexusMods API Key。也可通过环境变量 NEXUS_API_KEY 设置。

.PARAMETER GamePath
    游戏路径，用于收集 BepInEx/plugins 下的部署文件。
    如果不指定，只从项目目录收集。

.PARAMETER SkipBuild
    跳过构建步骤。

.PARAMETER SkipNexus
    跳过 NexusMods 上传。

.PARAMETER SkipGitHub
    跳过 GitHub Release。

.PARAMETER ReleaseNotes
    GitHub Release 说明内容。

.PARAMETER Prerelease
    GitHub 标记为预发布。

.EXAMPLE
    .\Release.ps1

.EXAMPLE
    .\Release.ps1 -SkipNexus -ReleaseNotes "修复了若干问题。"

.NOTES
    GitHub Release 需要安装 gh CLI: winget install GitHub.cli
#>
param(
    [string]$ModNamespace = "Bark",
    [string]$ModDisplayName = "Bark",
    [string]$ModVersion = "1.1.0",
    [string]$Configuration = "Release",
    [string]$NexusApiKey = $env:NEXUS_API_KEY,
    [string]$GamePath,
    [switch]$SkipBuild,
    [switch]$SkipNexus,
    [switch]$SkipGitHub,
    [string]$ReleaseNotes,
    [switch]$Prerelease
)

$ErrorActionPreference = "Stop"
$scriptDir = $PSScriptRoot

# ============================================================
# 辅助函数
# ============================================================

function Write-Step {
    param([string]$Message, [string]$Color = "Cyan")
    Write-Host ""
    Write-Host ">>> $Message" -ForegroundColor $Color
}

function Write-OK {
    param([string]$Message)
    Write-Host "    OK: $Message" -ForegroundColor Green
}

function Write-Fail {
    param([string]$Message)
    Write-Host "    FAIL: $Message" -ForegroundColor Red
}

function Convert-MarkdownToNexusBBCode {
    <# 将 Markdown 转换为 NexusMods BBCode 格式 #>
    param([string]$Markdown)
    
    $result = $Markdown
    
    # 标题 ## → [size=4][b][/b][/size], # → [size=5][b][/b][/size]
    $result = $result -replace '(?m)^###\s+(.+)$', '[size=3][b]$1[/b][/size]'
    $result = $result -replace '(?m)^##\s+(.+)$', '[size=4][b]$1[/b][/size]'
    $result = $result -replace '(?m)^#\s+(.+)$', '[size=5][b]$1[/b][/size]'
    
    # **粗体**
    $result = $result -replace '\*\*(.+?)\*\*', '[b]$1[/b]'
    
    # *斜体*
    $result = $result -replace '(?<!\*)\*(?!\*)(.+?)(?<!\*)\*(?!\*)', '[i]$1[/i]'
    
    # ~~删除线~~
    $result = $result -replace '~~(.+?)~~', '[s]$1[/s]'
    
    # `代码`
    $result = $result -replace '`([^`]+)`', '[code]$1[/code]'
    
    # [文本](URL) → [url=URL]文本[/url]
    $result = $result -replace '\[([^\]]+)\]\(([^)]+)\)', '[url=$2]$1[/url]'
    
    # - 无序列表
    $result = $result -replace '(?m)^-\s+(.+)$', '[*]$1[/*]'
    
    # 1. 有序列表
    $result = $result -replace '(?m)^\d+\.\s+(.+)$', '[*]$1[/*]'
    
    # > 引用
    $result = $result -replace '(?m)^>\s?(.+)$', '[quote]$1[/quote]'
    
    # --- 分隔线
    $result = $result -replace '(?m)^---\s*$', '[line]'
    
    return $result
}

# ============================================================
# 1. 模组信息（NewMod.ps1 已自动填入）
# ============================================================

Write-Step "模组信息:" "Yellow"
Write-OK "命名空间:   $ModNamespace"
Write-OK "显示名称:   $ModDisplayName"

# 询问版本号（读自 Plugin.cs 作为默认值）
$userVersion = Read-Host "输入版本号 (默认: $ModVersion)"
if (-not [string]::IsNullOrWhiteSpace($userVersion)) {
    $ModVersion = $userVersion
}
Write-OK "版本号:     $ModVersion"

$releasesDir = Join-Path $scriptDir "Releases"
if (-not (Test-Path $releasesDir)) {
    New-Item -ItemType Directory -Path $releasesDir -Force | Out-Null
}

$zipName = "$ModDisplayName-v$ModVersion.zip"
$zipPath = Join-Path $releasesDir $zipName

# ============================================================
# 2. 构建项目
# ============================================================

if (-not $SkipBuild) {
    Write-Step "构建项目 ($Configuration)..." "Yellow"
    
    Push-Location $scriptDir
    try {
        $buildResult = & dotnet build -c $Configuration 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Error "构建失败:`n$buildResult"
            exit 1
        }
        Write-OK "构建成功"
    } finally {
        Pop-Location
    }
}

# ============================================================
# 3. 收集文件并压缩
# ============================================================

Write-Step "收集文件并创建压缩包..." "Yellow"

$tempDir = Join-Path ([System.IO.Path]::GetTempPath()) "MossRelease_$([System.Guid]::NewGuid())"
$packageDir = Join-Path $tempDir "package"
New-Item -ItemType Directory -Path $packageDir -Force | Out-Null

# DLL 文件
$buildOutputDir = Join-Path $scriptDir "bin/$Configuration/net472"
$dllSource = Join-Path $buildOutputDir "$ModNamespace.dll"

if (Test-Path $dllSource) {
    Copy-Item $dllSource $packageDir -Force
    Write-OK "已添加: $ModNamespace.dll"
} else {
    Write-Warning "未找到 DLL: $dllSource"
}

# 文档文件 (含更新日志)
$docFiles = @("README.md", "README_ZH.md", "LICENSE.md", "CHANGELOG.md", "CHANGELOG_ZH.md", "Cover.png")
foreach ($doc in $docFiles) {
    $docPath = Join-Path $scriptDir $doc
    if (Test-Path $docPath) {
        Copy-Item $docPath $packageDir -Force
        Write-OK "已添加: $doc"
    }
}

# 如果未指定 ReleaseNotes，自动从 CHANGELOG.md 读取
if ([string]::IsNullOrWhiteSpace($ReleaseNotes)) {
    $changelogPath = Join-Path $scriptDir "CHANGELOG.md"
    if (Test-Path $changelogPath) {
        $changelogContent = Get-Content $changelogPath -Raw -Encoding UTF8
        # 截取当前版本 (## vVersion 到下一个 ## 之间)
        $pattern = "(##\s+v?$([regex]::Escape($ModVersion))[\s\S]*?)(?=##\s+v|`$)"
        if ($changelogContent -match $pattern) {
            $ReleaseNotes = $Matches[1].Trim()
            Write-OK "从 CHANGELOG.md 读取发布说明"
        } else {
            # 取前 20 行
            $ReleaseNotes = ($changelogContent -split "`n" | Select-Object -First 20) -join "`n"
            Write-OK "从 CHANGELOG.md 读取发布说明 (前 20 行)"
        }
    }
    
    # 转换为 NexusMods BBCode 格式
    if ($ReleaseNotes) {
        $NexusDescription = Convert-MarkdownToNexusBBCode -Markdown $ReleaseNotes
        Write-OK "已生成 NexusMods BBCode 发布说明"
    }
}

# 如果指定了 GamePath，也收集部署目录下的额外文件
if ($GamePath -and (Test-Path $GamePath)) {
    $deployedDir = Join-Path $GamePath "BepInEx/plugins/$ModDisplayName"
    if (Test-Path $deployedDir) {
        $extraFiles = Get-ChildItem $deployedDir -File | Where-Object { $_.Extension -ne ".dll" }
        foreach ($f in $extraFiles) {
            Copy-Item $f.FullName $packageDir -Force
            Write-OK "从部署目录添加: $($f.Name)"
        }
    }
}

# 创建压缩包
if (Get-Command Compress-Archive -ErrorAction SilentlyContinue) {
    Compress-Archive -Path "$packageDir/*" -DestinationPath $zipPath -Force
} else {
    Write-Error "Compress-Archive 不可用"
    exit 1
}

$zipSize = (Get-Item $zipPath).Length
$zipSizeMB = [math]::Round($zipSize / 1MB, 2)
Write-OK "压缩包: $zipName ($zipSizeMB MB)"

# 清理临时目录
Remove-Item -Recurse -Force $tempDir -ErrorAction SilentlyContinue

# ============================================================
# 4. 上传到 NexusMods
# ============================================================

if (-not $SkipNexus) {
    Write-Step "上传到 NexusMods..." "Yellow"

    if ([string]::IsNullOrWhiteSpace($NexusApiKey)) {
        Write-Fail "未设置 NexusMods API Key。设置环境变量 NEXUS_API_KEY 或使用 -NexusApiKey 参数。"
    } else {
        $nexusBase = "https://api.nexusmods.com/v3"
        $nexusModId = 362

        $nexusHeaders = @{
            "apikey" = $NexusApiKey
            "Accept" = "application/json"
        }

        try {
            # 4a. 创建上传会话
            Write-Host "    创建上传会话..." -ForegroundColor DarkGray
            $createUploadBody = @{
                filename   = $zipName
                size_bytes = $zipSize
            } | ConvertTo-Json

            $uploadSession = Invoke-RestMethod -Uri "$nexusBase/uploads" `
                -Method Post -Headers $nexusHeaders `
                -Body $createUploadBody -ContentType "application/json"

            $uploadId = $uploadSession.data.id
            $presignedUrl = $uploadSession.data.presigned_url
            Write-OK "上传会话已创建: $uploadId"

            # 4b. PUT 文件到 S3 预签名 URL
            Write-Host "    上传文件中 ($zipSizeMB MB)..." -ForegroundColor DarkGray
            
            $putClient = [System.Net.Http.HttpClient]::new()
            $fileBytes = [System.IO.File]::ReadAllBytes($zipPath)
            $byteContent = [System.Net.Http.ByteArrayContent]::new($fileBytes)
            $byteContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("application/zip")
            $byteContent.Headers.TryAddWithoutValidation("Content-Disposition", "") | Out-Null
            $byteContent.Headers.ContentLength = $fileBytes.Length
            
            $putResponse = $putClient.PutAsync($presignedUrl, $byteContent).Result
            
            if (-not $putResponse.IsSuccessStatusCode) {
                $putBody = $putResponse.Content.ReadAsStringAsync().Result
                # 已知问题: NexusMods v3 API S3 预签名 URL 签名校验存在服务端 bug
                # 规范请求与实际请求完全匹配但签名持续失败
                Write-Host "    [已知问题] S3 预签名 URL 签名不匹配 (NexusMods API bug)" -ForegroundColor Yellow
                Write-Host "    压缩包已生成: $zipPath" -ForegroundColor Yellow
                Write-Host "    请通过 NexusMods 网页手动上传。" -ForegroundColor Yellow
                throw "S3 PUT 失败: $($putResponse.StatusCode) - 请手动上传到 NexusMods"
            }
            
            Write-OK "文件已上传"

            # 4c. 确认上传
            Write-Host "    确认上传..." -ForegroundColor DarkGray
            Invoke-RestMethod -Uri "$nexusBase/uploads/$uploadId/finalise" `
                -Method Post -Headers $nexusHeaders | Out-Null
            Write-OK "上传已确认"

            # 4d. 创建 Mod 文件条目
            Write-Host "    创建 Mod 文件条目..." -ForegroundColor DarkGray
            $createFileBody = @{
                upload_id     = $uploadId
                mod_id        = $nexusModId
                name          = "$ModDisplayName v$ModVersion"
                version       = $ModVersion
                file_category = 1
            }
            if ($NexusDescription) {
                $createFileBody["description"] = $NexusDescription
            }

            $modFile = Invoke-RestMethod -Uri "$nexusBase/mod-files" `
                -Method Post -Headers $nexusHeaders `
                -Body ($createFileBody | ConvertTo-Json) `
                -ContentType "application/json"

            Write-OK "Mod 文件已创建 (ID: $($modFile.data.id))"
            Write-OK "NexusMods 上传完成!"

        } catch {
            Write-Fail "NexusMods 上传失败: $_"
            if ($_.Exception.Response) {
                try {
                    $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
                    $errorBody = $reader.ReadToEnd()
                    Write-Host "    API 响应: $errorBody" -ForegroundColor Red
                } catch {}
            }
        }
    }
}

# ============================================================
# 5. 上传到 GitHub Release
# ============================================================

if (-not $SkipGitHub) {
    Write-Step "上传到 GitHub Release..." "Yellow"

    $ghAvailable = Get-Command gh -ErrorAction SilentlyContinue
    if (-not $ghAvailable) {
        Write-Fail "gh CLI 未安装。请运行: winget install GitHub.cli"
    } else {
        try {
            # 检查是否已认证
            $ghAuth = & gh auth status 2>&1
            if ($LASTEXITCODE -ne 0) {
                Write-Fail "gh 未认证。请运行: gh auth login"
            } else {
                $tagName = "v$ModVersion"

                # 构建 gh release create 参数
                $ghArgs = @(
                    "release", "create", $tagName,
                    $zipPath,
                    "--title", "$ModDisplayName $tagName"
                )

                if ($ReleaseNotes) {
                    $ghArgs += @("--notes", $ReleaseNotes)
                } else {
                    $ghArgs += @("--generate-notes")
                }

                if ($Prerelease) {
                    $ghArgs += "--prerelease"
                }

                Write-Host "    执行: gh $($ghArgs -join ' ')" -ForegroundColor DarkGray
                & gh @ghArgs

                if ($LASTEXITCODE -eq 0) {
                    Write-OK "GitHub Release 已创建: $tagName"
                } else {
                    Write-Fail "GitHub Release 创建失败 (exit code: $LASTEXITCODE)"
                }
            }
        } catch {
            Write-Fail "GitHub Release 失败: $_"
        }
    }
}

# ============================================================
# 完成
# ============================================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  发布完成!" -ForegroundColor Green
Write-Host "  压缩包: $zipName" -ForegroundColor White
Write-Host "  大小:   $zipSizeMB MB" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
