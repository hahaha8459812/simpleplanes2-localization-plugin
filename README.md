# SimplePlanes 2 本地化插件

[English](README.en.md)

这是一个用于 `SimplePlanes 2` 的运行时本地化插件。目前主要提供简体中文汉化，覆盖游戏主菜单、设计器、零件列表、零件属性、悬浮说明、上传发布页面等内容。

插件基于 `BepInEx + Harmony`，安装后启动游戏即可自动汉化，不需要修改游戏文件，也不需要玩家本地编译。

## 玩家使用教程

### 下载

普通玩家请下载发布页里的：

```text
SimplePlanes2TranslationMod-Release.zip
```

不要下载 `Dev` 包。`Dev` 包用于翻译采集，会生成额外的文本记录文件。

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

如果游戏不在默认 Steam 路径，手动指定游戏目录：

```powershell
.\install.ps1 -GameDir "D:\SteamLibrary\steamapps\common\SimplePlanes 2"
```

如果 PowerShell 阻止脚本运行，在解压目录打开 PowerShell，先执行：

```powershell
Set-ExecutionPolicy -Scope Process Bypass
```

安装成功后，插件会位于：

```text
SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation
```

### 使用

启动游戏后会自动汉化。

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

如果你还安装了其他 BepInEx Mod，不要删除整个 `BepInEx` 目录，只删除本汉化插件目录。

## 常见问题

### 游戏里还有英文

先按一次 `F6`。如果仍然是英文，说明该文本可能还没有被翻译，可以截图反馈。

### 中文显示成方框

确认插件目录里存在字体文件：

```text
BepInEx\plugins\SimplePlanes2Translation\fonts\SourceHanSansSC-Regular.otf
```

如果文件缺失，重新安装 Release 包。

### 游戏启动变慢

首次加载 BepInEx 和中文字体时可能会慢一些。后续启动通常会更稳定。

### 安装脚本提示找不到游戏

用 `-GameDir` 手动指定 `SimplePlanes 2.exe` 所在目录，不要指定到 `SimplePlanes 2_Data`。

## 项目文档

- [项目说明](translation-mod/README.md)
- [翻译维护流程](translation-mod/docs/TRANSLATION_WORKFLOW.md)
- [发布检查清单](translation-mod/docs/RELEASE_CHECKLIST.md)
- [术语表](translation-mod/content/translations/zh-CN.fragments/TERMS.md)

## 开发与发版

源码和构建脚本位于：

```text
translation-mod/
```

本地构建：

```powershell
cd translation-mod
.\build.ps1
```

生成的分发包位于：

```text
translation-mod\release
```

项目也提供 GitHub Actions 发版工作流，但编译需要本机安装的 `SimplePlanes 2_Data\Managed` 程序集，因此需要 Windows self-hosted runner，普通 GitHub-hosted runner 无法直接编译。
