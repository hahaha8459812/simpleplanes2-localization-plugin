# SimplePlanes 2 本地化插件

[English](README.en.md)

这是一个用于 `SimplePlanes 2` 的运行时本地化插件。目前主要提供简体中文汉化，覆盖主菜单、设计器、零件列表、零件属性、悬浮说明、设置、上传发布页面和部分飞行界面。

插件基于 `BepInEx + Harmony`。发行包只包含插件本体和必要资源，不内置 BepInEx；手动解压部署。

## 玩家使用

### 下载

普通玩家请在 GitHub Releases 下载：

```text
SimplePlanes2TranslationMod-Release.zip
```

不要下载 `Dev` 包。`Dev` 包用于翻译采集，会生成额外的文本记录文件。

### 安装 BepInEx

发布包不含 BepInEx，需要单独安装。已经装好 BepInEx 5 的话跳到「安装本插件」。

本插件在 BepInEx 5.4.23.5（Mono x64）上测试过，请使用 [5.4.x 版本的 BepInEx](https://github.com/BepInEx/BepInEx/releases)。下载 [BepInEx_win_x64_5.4.23.5.zip](https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.5/BepInEx_win_x64_5.4.23.5.zip)，解压到游戏根目录，也就是 `SimplePlanes 2.exe` 所在的目录：

```text
SimplePlanes 2\
├─ winhttp.dll
├─ doorstop_config.ini
├─ .doorstop_version
└─ BepInEx\
```

启动一次游戏再退出，`BepInEx\plugins\` 与 `BepInEx\config\` 会自动生成。

### 安装本插件

1. 关闭 `SimplePlanes 2`。
2. 解压 `SimplePlanes2TranslationMod-Release.zip`。
3. 把压缩包里的 `BepInEx` 文件夹放进游戏根目录。
4. 启动游戏。

安装成功后，插件会位于：

```text
SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation
```

### 使用

启动游戏后会自动汉化。

快捷键：

- `F1`：临时开关汉化，方便对照原文和截图反馈。
- `F2`：重新加载汉化配置和词表。纯文本翻译更新后可以按这个键热重载。

正常游玩不需要打开任何额外程序。

### 卸载

关闭游戏后，删除：

```text
SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation
```

如果你只为了这个汉化安装了 BepInEx，可以手动删除游戏目录下的这些文件和目录：

```text
BepInEx
.doorstop_version
changelog.txt
doorstop_config.ini
winhttp.dll
```

如果你还安装了其他 BepInEx Mod，不要删除整个 `BepInEx` 目录，只删除本插件目录。

### 常见问题

#### 游戏里还有英文

先按一次 `F2`。如果仍然是英文，说明该文本可能还没有被翻译，可以截图反馈。

#### 中文显示成方框

确认插件目录里存在字体文件：

```text
BepInEx\plugins\SimplePlanes2Translation\fonts\SourceHanSansSC-Regular.otf
```

如果文件缺失，重新安装 Release 包。

#### 游戏启动变慢

首次加载 BepInEx 和中文字体时可能会慢一些。后续启动通常会更稳定。

#### 快捷键没反应

确认当前安装的是较新的 Release 包。旧版本使用过 `F6/F10`、`Alt + +/-` 等快捷键。

## 参考文档

- [开发与构建](translation-mod/docs/DEVELOPMENT.md)：项目结构、构建、运行模式、发布。
- [翻译维护流程](translation-mod/docs/TRANSLATION_WORKFLOW.md)：翻译流程、翻译规则、数据格式与可提取的文本来源。
- [发布检查清单](translation-mod/docs/RELEASE_CHECKLIST.md)。
