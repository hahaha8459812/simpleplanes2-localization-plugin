# SimplePlanes 2 中文翻译 Mod

[English](README.en.md)

这是一个给 `SimplePlanes 2` 使用的中文翻译 Mod。项目使用 `BepInEx + Harmony` 注入游戏运行时，拦截和扫描 Unity UI 文本，再用本地 JSON 词表替换为中文。

当前目标是把游戏主要 UI、设计器、零件列表、悬浮说明和上传发布流程翻译到可玩、可维护的程度，再逐步精修术语和语境。

## 玩家使用教程

### 下载

普通玩家下载：

```text
SimplePlanes2TranslationMod-Release.zip
```

不要下载 `SimplePlanes2TranslationMod-Dev.zip`。`Dev` 包用于翻译采集，会生成额外的文本记录文件。

### 安装

1. 关闭 `SimplePlanes 2`。
2. 解压 `SimplePlanes2TranslationMod-Release.zip`。
3. 把解压出来的全部内容放进 `SimplePlanes 2` 游戏根目录，也就是 `SimplePlanes 2.exe` 所在目录。
4. 启动游戏。

Release 包本身已经按游戏根目录结构打包，所以最省事的方式就是直接把压缩包内容解压到游戏本地文件夹。

如果不想手动复制，也可以运行备用安装脚本：

```powershell
.\install.ps1
```

如果游戏不在默认路径：

```powershell
.\install.ps1 -GameDir "D:\SteamLibrary\steamapps\common\SimplePlanes 2"
```

如果 PowerShell 阻止脚本运行，在解压目录打开 PowerShell，先执行：

```powershell
Set-ExecutionPolicy -Scope Process Bypass
```

安装后文件会进入：

```text
BepInEx\plugins\SimplePlanes2Translation
```

### 使用

启动游戏即可自动汉化。

快捷键：

- `F6`：重新加载汉化配置和词表。
- `F10`：临时开关汉化，方便对照原文和截图反馈。

正常游玩不需要打开任何额外程序。

### 卸载

关闭游戏后，删除：

```text
SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation
```

如果你只为了这个汉化安装了 BepInEx，也可以删除游戏目录下的这些文件和目录：

```text
BepInEx
.doorstop_version
changelog.txt
doorstop_config.ini
winhttp.dll
```

如果你还装了其他 BepInEx Mod，不要删除整个 `BepInEx` 目录，只删除本汉化插件目录。

### 常见问题

游戏里还有英文：先按一次 `F6`。如果仍然是英文，说明该文本可能还没有被翻译，可以截图反馈。

中文显示成方框：确认插件目录里存在 `fonts\SourceHanSansSC-Regular.otf`。如果文件缺失，重新安装 Release 包。

游戏启动变慢：首次加载 BepInEx 和中文字体时可能会慢一些。后续启动通常会更稳定。

安装脚本提示找不到游戏：用 `-GameDir` 手动指定 `SimplePlanes 2.exe` 所在目录，不要指定到 `SimplePlanes 2_Data`。

## 功能

- 运行时翻译 `TextWidget.SetText()` 和独立 `TextMeshProUGUI` 文本。
- 支持 `collect`、`translate`、`hybrid` 三种模式。
- 开发模式会记录文本、对象路径、父节点、兄弟序号和位置，方便定位同名文本。
- 内置思源黑体简体中文字体，解决中文方框和混合字体问题。
- `F6` 热重载设置和翻译词表。
- `F10` 临时开关翻译。
- 分发包保持单文件夹结构，并提供一键安装脚本。

## 目录

```text
translation-mod/
  build.ps1                         构建、合并词表、生成分发包
  install.ps1                       分发包内的一键安装脚本
  CHANGELOG.md                      当前阶段变更记录
  docs/
    USER_GUIDE.md                   玩家安装和使用说明
    USER_GUIDE.en.md                Player guide
    RELEASE_CHECKLIST.md            发布前检查清单
    RELEASE_CHECKLIST.en.md         Release checklist
    TRANSLATION_WORKFLOW.md         采集、翻译、热更新流程
    TRANSLATION_WORKFLOW.en.md      Translation workflow
  content/
    settings.dev.json               开发采集模式配置
    settings.release.json           正常分发模式配置
    translations/
      zh-CN.json                    构建生成的合并词表
      zh-CN.fragments/              可编辑的翻译源文件
  src/                              Mod 源码
  artifacts/                        构建产物，自动生成
  release/                          分发包，自动生成
```

`zh-CN.fragments/*.json` 是翻译源文件。不要直接维护 `zh-CN.json`，它会被 `build.ps1` 重新生成。

## 构建

默认游戏路径是：

```powershell
E:\Game\steam\steamapps\common\SimplePlanes 2
```

在项目目录运行：

```powershell
.\build.ps1
```

如果游戏装在别的位置：

```powershell
.\build.ps1 -GameDir "D:\SteamLibrary\steamapps\common\SimplePlanes 2"
```

构建完成后会生成：

- `release/SimplePlanes2TranslationMod-Dev`
- `release/SimplePlanes2TranslationMod-Dev.zip`
- `release/SimplePlanes2TranslationMod-Release`
- `release/SimplePlanes2TranslationMod-Release.zip`

`Dev` 包用于采集文本，`Release` 包用于普通玩家安装。

## GitHub Actions 发版

项目带有 `.github/workflows/release.yml`，可以在 GitHub 上自动构建并发布压缩包。

当前源码需要引用 `SimplePlanes 2_Data\Managed` 里的游戏程序集，所以不能直接使用普通 GitHub 托管 runner 编译。推荐使用一台已安装游戏的 Windows self-hosted runner。

runner 要求：

- Windows x64。
- 已安装 `SimplePlanes 2`。
- 能访问游戏目录里的 `SimplePlanes 2_Data\Managed`。
- 能运行 Windows PowerShell 和 .NET Framework `csc.exe`。

如果 self-hosted runner 上的游戏路径不同，在 GitHub 仓库设置里新增 repository variable：

```text
SP2_GAME_DIR=D:\SteamLibrary\steamapps\common\SimplePlanes 2
```

发版方式：

- 推送 `v*` tag，例如 `v0.1.0`，会自动构建并创建 GitHub Release。
- 在 Actions 页面手动运行 `Build release packages`，可以只构建 artifact，也可以填写 `release_tag` 来发布。

## 开发流程

常用流程是：

1. 安装 `Dev` 包，或把游戏目录里的 `settings.json` 切到 `hybrid`。
2. 进游戏走一遍目标页面、按钮和悬浮说明。
3. 退出游戏，读取插件目录里的 `captured-texts.json` 和 `missing-texts.txt`。
4. 把稳定 UI 文本补进 `content/translations/zh-CN.fragments/*.json`。
5. 运行 `build.ps1` 合并词表。
6. 只复制新的 `content/translations/zh-CN.json` 到游戏插件目录。
7. 游戏里按 `F6` 验证。

更详细的维护流程见 [翻译维护流程](docs/TRANSLATION_WORKFLOW.md)。

## 翻译维护

- `20-26`：设计器各面板和工具。
- `27`：零件列表分类标题。
- `28`：悬浮说明、搜索和长文本。
- `29`：零件短名。
- `30-32`：上传、存档、设置等对话框。
- `99`：暂时无法归类但需要保留的文本。

同一个英文词在不同位置需要不同翻译时，使用 `contextEntries`，通过 `gameObjectPathContains` 等上下文条件区分。

术语约定见 [术语表](content/translations/zh-CN.fragments/TERMS.md)。

## 注意

- 不要把用户作品名、存档名、零件实例名和数值结果硬编码进词表。
- 带 `#123`、重量、容量、坐标、颜色编号之类的文本通常是动态内容。
- 文本纯修改通常只需要热更新 `zh-CN.json`，不需要重启游戏。
- 修改 C# 源码后需要重新构建并重启游戏，因为 DLL 只在游戏启动时加载。
