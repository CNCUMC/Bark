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
    [string]$ModNamespace = "__MOD_NAMESPACE__",
    [string]$ModDisplayName = "__MOD_DISPLAY_NAME__",
    [string]$ModVersion = "__MOD_VERSION__",
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
# 编码设置
# ============================================================
$OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::InputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
chcp 65001 > $null

# ============================================================
# 辅助函数
# ============================================================

function Write-Step
{
    param([string]$Message, [string]$Color = "Cyan")
    Write-Host ""
    Write-Host ">>> $Message" -ForegroundColor $Color
}

function Write-OK
{
    param([string]$Message)
    Write-Host "    OK: $Message" -ForegroundColor Green
}

function Write-Fail
{
    param([string]$Message)
    Write-Host "    FAIL: $Message" -ForegroundColor Red
}

# ============================================================
# 1. 模组信息（NewMod.ps1 已自动填入）
# ============================================================

Write-Step "模组信息:" "Yellow"
Write-OK "命名空间:   $ModNamespace"
Write-OK "显示名称:   $ModDisplayName"
Write-OK "版本号:     $ModVersion"

$zipName = "$ModDisplayName-v$ModVersion.zip"
$zipPath = Join-Path $scriptDir $zipName

# ============================================================
# 2. 构建项目
# ============================================================

if (-not $SkipBuild)
{
    Write-Step "构建项目 ($Configuration)..." "Yellow"

    Push-Location $scriptDir
    try
    {
        $buildResult = & dotnet build -c $Configuration 2>&1
        if ($LASTEXITCODE -ne 0)
        {
            Write-Error "构建失败:`n$buildResult"
            exit 1
        }
        Write-OK "构建成功"
    }
    finally
    {
        Pop-Location
    }
}

# ============================================================
# 3. 收集文件并压缩
# ============================================================

Write-Step "收集文件并创建压缩包..." "Yellow"

$tempDir = Join-Path ([System.IO.Path]::GetTempPath()) "MossRelease_$([System.Guid]::NewGuid() )"
$packageDir = Join-Path $tempDir "package"
New-Item -ItemType Directory -Path $packageDir -Force | Out-Null

# DLL 文件
$buildOutputDir = Join-Path $scriptDir "bin/$Configuration/net472"
$dllSource = Join-Path $buildOutputDir "$ModNamespace.dll"

if (Test-Path $dllSource)
{
    Copy-Item $dllSource $packageDir -Force
    Write-OK "已添加: $ModNamespace.dll"
}
else
{
    Write-Warning "未找到 DLL: $dllSource"
}

# 文档文件 (含更新日志)
$docFiles = @("README.md", "README_ZH.md", "LICENSE.md", "CHANGELOG.md", "CHANGELOG_ZH.md", "Cover.png")
foreach ($doc in $docFiles)
{
    $docPath = Join-Path $scriptDir $doc
    if (Test-Path $docPath)
    {
        Copy-Item $docPath $packageDir -Force
        Write-OK "已添加: $doc"
    }
}

# 如果未指定 ReleaseNotes，自动从 CHANGELOG.md 读取
if ( [string]::IsNullOrWhiteSpace($ReleaseNotes))
{
    $changelogPath = Join-Path $scriptDir "CHANGELOG.md"
    if (Test-Path $changelogPath)
    {
        $changelogContent = Get-Content $changelogPath -Raw -Encoding UTF8
        # 截取当前版本 (## vVersion 到下一个 ## 之间)
        $pattern = "(##\s+v?$([regex]::Escape($ModVersion) )[\s\S]*?)(?=##\s+v|`$)"
        if ($changelogContent -match $pattern)
        {
            $ReleaseNotes = $Matches[1].Trim()
            Write-OK "从 CHANGELOG.md 读取发布说明"
        }
        else
        {
            # 取前 20 行
            $ReleaseNotes = ($changelogContent -split "`n" | Select-Object -First 20) -join "`n"
            Write-OK "从 CHANGELOG.md 读取发布说明 (前 20 行)"
        }
    }
}

# 如果指定了 GamePath，也收集部署目录下的额外文件
if ($GamePath -and (Test-Path $GamePath))
{
    $deployedDir = Join-Path $GamePath "BepInEx/plugins/$ModDisplayName"
    if (Test-Path $deployedDir)
    {
        $extraFiles = Get-ChildItem $deployedDir -File | Where-Object { $_.Extension -ne ".dll" }
        foreach ($f in $extraFiles)
        {
            Copy-Item $f.FullName $packageDir -Force
            Write-OK "从部署目录添加: $( $f.Name )"
        }
    }
}

# 创建压缩包
if (Get-Command Compress-Archive -ErrorAction SilentlyContinue)
{
    Compress-Archive -Path "$packageDir/*" -DestinationPath $zipPath -Force
}
else
{
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

if (-not $SkipNexus)
{
    Write-Step "上传到 NexusMods..." "Yellow"

    if ( [string]::IsNullOrWhiteSpace($NexusApiKey))
    {
        Write-Fail "未设置 NexusMods API Key。设置环境变量 NEXUS_API_KEY 或使用 -NexusApiKey 参数。"
    }
    else
    {
        $nexusBase = "https://api.nexusmods.com/v1"
        $headers = @{
            "ApiKey" = $NexusApiKey
            "Content-Type" = "application/json"
        }

        try
        {
            # Step 1: 创建上传会话
            Write-Host "    创建上传会话..." -ForegroundColor DarkGray
            $createBody = @{
                size_bytes = $zipSize
                filename = $zipName
            } | ConvertTo-Json

            $createResult = Invoke-RestMethod -Uri "$nexusBase/uploads" -Method Post -Headers $headers -Body $createBody

            if (-not $createResult.data -or -not $createResult.data.presigned_url)
            {
                Write-Fail "创建上传会话失败: $( $createResult | ConvertTo-Json )"
                exit 1
            }

            $uploadId = $createResult.data.id
            $presignedUrl = $createResult.data.presigned_url
            Write-OK "上传会话已创建: $uploadId"

            # Step 2: PUT 文件到 presigned URL
            Write-Host "    上传文件中 ($zipSizeMB MB)..." -ForegroundColor DarkGray
            $fileBytes = [System.IO.File]::ReadAllBytes($zipPath)
            $putResult = Invoke-RestMethod -Uri $presignedUrl -Method Put -Body $fileBytes -ContentType "application/octet-stream"
            Write-OK "文件上传完成"

            # Step 3: 完成上传
            Write-Host "    完成上传会话..." -ForegroundColor DarkGray
            $finaliseResult = Invoke-RestMethod -Uri "$nexusBase/uploads/$uploadId/finalise" -Method Post -Headers $headers
            Write-OK "上传已提交，状态: $( $finaliseResult.data.state )"

        }
        catch
        {
            Write-Fail "NexusMods 上传失败: $_"
            Write-Host "    $( $_.Exception.Response )" -ForegroundColor Red
        }
    }
}

# ============================================================
# 5. 上传到 GitHub Release
# ============================================================

if (-not $SkipGitHub)
{
    Write-Step "上传到 GitHub Release..." "Yellow"

    $ghAvailable = Get-Command gh -ErrorAction SilentlyContinue
    if (-not $ghAvailable)
    {
        Write-Fail "gh CLI 未安装。请运行: winget install GitHub.cli"
    }
    else
    {
        try
        {
            # 检查是否已认证
            $ghAuth = & gh auth status 2>&1
            if ($LASTEXITCODE -ne 0)
            {
                Write-Fail "gh 未认证。请运行: gh auth login"
            }
            else
            {
                $tagName = "v$ModVersion"

                # 构建 gh release create 参数
                $ghArgs = @(
                    "release", "create", $tagName,
                    $zipPath,
                    "--title", "$ModName $tagName"
                )

                if ($ReleaseNotes)
                {
                    $ghArgs += @("--notes", $ReleaseNotes)
                }
                else
                {
                    $ghArgs += @("--generate-notes")
                }

                if ($Prerelease)
                {
                    $ghArgs += "--prerelease"
                }

                Write-Host "    执行: gh $( $ghArgs -join ' ' )" -ForegroundColor DarkGray
                & gh @ghArgs

                if ($LASTEXITCODE -eq 0)
                {
                    Write-OK "GitHub Release 已创建: $tagName"
                }
                else
                {
                    Write-Fail "GitHub Release 创建失败 (exit code: $LASTEXITCODE)"
                }
            }
        }
        catch
        {
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
