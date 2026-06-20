# Moss-Template

一个用于开发 `Casualties Unknown` 模组的 [dotnet new](https://learn.microsoft.com/zh-cn/dotnet/core/tools/dotnet-new)
模板。

基于 [05126619z/ScavTemplate](https://github.com/05126619z/ScavTemplate)。

---

## 快速开始

### 方式一：使用 `NewMod.ps1`（推荐）

1. 克隆本仓库并注册模板：

```powershell
git clone https://github.com/CNCUMC/Moss-Template.git
cd Moss-Template
dotnet new install .
```

2. 在任意目录运行创建脚本：

```powershell
cd E:/Projects  # 你想创建项目的目录
<path-to>\NewMod.ps1
```

脚本会自动：

- 搜索 Steam 安装路径中的 Casualties Unknown 游戏目录
- 交互式提示输入模组名称、GUID、版本等信息
- 调用 `dotnet new bark` 生成项目
- 所有文件名和内容自动替换完成

3. 构建并测试：

```powershell
cd MyCoolMod
dotnet build
```

### 方式二：使用 `dotnet new` 命令

注册模板后（见上方第 1 步），直接使用命令行：

```powershell
dotnet new bark -n MyCoolMod `
    --ModDisplayName "My Cool Mod" `
    --ModGuid "com.example.mycoolmod" `
    --ModVersion "1.0.0" `
    --AuthorName "Your Name" `
    --GameManagedDir "E:/SteamLibrary/steamapps/common/Casualties Unknown Demo/CasualtiesUnknown_Data/Managed"
```

### 方式三：从 GitHub 克隆（传统方式）

1. 在 GitHub 上点击 [Use this template](https://github.com/new?template_name=Moss-Template) 创建仓库
2. 克隆仓库后手动替换文件名和内容中的 `Bark`
3. 参考下方手动配置步骤

---

## 模板参数说明

| 参数                 | 说明                             | 默认值                 |
|--------------------|--------------------------------|---------------------|
| `-n` / `--name`    | 项目名称（PascalCase，如 `MyCoolMod`） | 必填                  |
| `--ModDisplayName` | 模组显示名称（如 `My Cool Mod`）        | 从名称自动生成             |
| `--ModGuid`        | 模组唯一标识（格式 `yourname.modname`）  | `com.example.mymod` |
| `--ModVersion`     | 初始版本号                          | `1.0.0`             |
| `--AuthorName`     | 作者名称（用于 LICENSE）               | `Your Name`         |
| `--GameManagedDir` | 游戏 Managed 目录路径                | Steam 默认路径          |

模板会自动替换以下内容：

- `Bark.csproj` → `{项目名}.csproj`
- `namespace Bark` → `namespace {项目名}`
- `org.cucnmc.bark` → `{ModGuid}`
- `Bark` → `{ModDisplayName}`
- 版本号、LICENSE 作者名、csproj 中的游戏 DLL 路径

---

## 关于 StartGame.ps1

[StartGame.ps1](StartGame.ps1) 会将编译好的 DLL 文件复制到游戏目录下的 BepInEx 插件目录，并自动启动游戏。

**参数：**

- `$GamePath` — 游戏安装目录（如 `E:/SteamLibrary/steamapps/common/Casualties Unknown Demo`）
- `$ModNamespace` — 模组命名空间（如 `MyCoolMod`）

**命令行运行：**

```powershell
.\StartGame.ps1 -GamePath "E:/SteamLibrary/steamapps/common/Casualties Unknown Demo" -ModNamespace "MyCoolMod"
```

### JetBrains Rider 配置

1. 右键 [StartGame.ps1](StartGame.ps1) → `运行 'StartGame.ps1'`
2. 点击编辑器右上角构建按钮旁的 `StartGame.ps1` 按钮 → `编辑配置...`
3. 填写 `Script arguments:`：`"E:/SteamLibrary/steamapps/common/Casualties Unknown Demo" "MyCoolMod"`
4. 设置 `Command parameters`：`-ExecutionPolicy Bypass`
5. 点击 `执行前` 旁的加号 → `构建解决方案` → 确定

之后每次按绿三角按钮即可自动构建、复制 DLL、启动游戏。

### Visual Studio

右键 [StartGame.ps1](StartGame.ps1) 选择 `运行`，手动填写参数。具体配置方式请自行研究。:P

---

## csproj 引用说明

模板包含 15 个核心游戏 DLL 引用。如需额外引用（如动画、音频、粒子等），在 csproj 中取消注释或添加新条目：

```xml
<!-- 例如：添加音频模块 -->
<Reference Include="UnityEngine.AudioModule">
    <HintPath>F:/SteamLibrary/steamapps/common/Casualties Unknown Demo/CasualtiesUnknown_Data/Managed/UnityEngine.AudioModule.dll</HintPath>
</Reference>
```

将 `F:/SteamLibrary/steamapps/common/Casualties Unknown Demo/CasualtiesUnknown_Data/Managed` 替换为你的游戏 Managed 目录实际路径。
