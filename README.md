# SimplePlanes 2 本地化插件

[English](README.en.md)

这是一个用于 `SimplePlanes 2` 的运行时本地化插件。目前主要提供简体中文汉化，覆盖主菜单、设计器、零件列表、零件属性、悬浮说明、设置、上传发布页面和部分飞行界面。

插件基于 `BepInEx + Harmony`。普通玩家只需要安装 Release 包并启动游戏，不需要修改游戏文件，也不需要本地编译。

## 玩家使用

### 下载

普通玩家请在 GitHub Releases 下载：

```text
SimplePlanes2TranslationMod-Release.zip
```

不要下载 `Dev` 包。`Dev` 包用于翻译采集，会生成额外的文本记录文件。

### 安装

1. 关闭 `SimplePlanes 2`。
2. 解压 `SimplePlanes2TranslationMod-Release.zip`。
3. 把压缩包里的全部内容放进 `SimplePlanes 2` 游戏根目录，也就是 `SimplePlanes 2.exe` 所在目录。
4. 启动游戏。

Release 包已经按游戏根目录结构打包，最省事的方式就是直接解压到 Steam 的“浏览本地文件”目录。

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

- `F1`：临时开关汉化，方便对照原文和截图反馈。
- `F2`：重新加载汉化配置和词表。纯文本翻译更新后可以按这个键热重载。

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

## 开发者与翻译维护

### 项目结构

```text
translation-mod/
  src/                                  # BepInEx/Harmony 插件源码
  content/settings.release.json          # 分发版设置，默认 translate
  content/settings.dev.json              # 开发采集版设置，默认 collect
  content/translations/zh-CN.fragments/  # 翻译源文件，按页面和功能拆分
  content/translations/zh-CN.json        # 构建生成的合并词表，不要手工编辑
  docs/                                  # 辅助文档和发布检查清单
  build.ps1                              # 构建 DLL、合并词表、打包 Release/Dev
```

翻译源只编辑：

```text
translation-mod/content/translations/zh-CN.fragments/*.json
```

不要直接编辑：

```text
translation-mod/content/translations/zh-CN.json
```

`zh-CN.json` 会由 `build.ps1` 重新生成。

### 构建

```powershell
cd translation-mod
.\\build.ps1
```

构建产物：

```text
translation-mod/artifacts/SimplePlanes2Translation.dll
translation-mod/release/SimplePlanes2TranslationMod-Release.zip
translation-mod/release/SimplePlanes2TranslationMod-Dev.zip
```

如果只修改翻译文本，可以构建后把新的 `zh-CN.json` 复制到游戏插件目录，再在游戏内按 `F2` 热重载。

如果修改了 C# 代码，需要重新构建 DLL，并在游戏退出后覆盖 DLL，再重新启动游戏。

### 运行模式

插件设置位于：

```text
SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation\settings.json
```

常用模式：

- `translate`：只翻译，不记录缺失文本。普通玩家使用。
- `collect`：只采集文本，不翻译。适合第一次粗采集。
- `hybrid`：一边翻译一边采集。适合边玩边补漏。

开发时常用 `hybrid`，因为它能保留现有汉化效果，同时记录新出现的文本。

### 标准翻译流程

1. 确认游戏正在使用开发设置，通常为 `hybrid` 模式，并开启 `LogMissingTexts` 与 `CaptureStandaloneTmpTexts`。
2. 清空或归档旧的 `captured-texts.json` 与 `missing-texts.txt`，避免不同页面的文本混在一起。
3. 启动游戏，在目标页面按 `F2` 重载设置，然后完整走一遍目标 UI、悬浮窗、下拉选项和相关属性页。
4. 退出游戏或等待采集文件落盘，查看插件目录里的 `captured-texts.json` 与 `missing-texts.txt`。
5. 按对象路径、父节点路径、场景名和控件位置判断文本语境。优先使用页面逻辑整理，不要只按英文原文堆在一起。
6. 判断文本类型：固定 UI 用 `entries`；同词不同义用 `contextEntries`；带数值的稳定模板用 `dynamicSuffixEntries` 或 `dynamicPrefixEntries`。
7. 明确跳过玩家作品名、存档名、零件实例名、服务器名、标签、坐标、重量、容量、百分比数值和颜色编号。
8. 在对应的 `zh-CN.fragments/*.json` 中补翻译。新增术语时保持和既有术语一致，必要时同步更新术语表。
9. 运行 `.\build.ps1`，让脚本验证重复键并重新生成 `zh-CN.json`。
10. 复制新的 `zh-CN.json` 到游戏插件目录，游戏内按 `F2` 验证。若修改了 DLL，则退出游戏后覆盖 DLL 再验证。
11. 验证通过后，把运行模式切回 `translate`，关闭缺失文本记录，避免分发版继续写采集文件。
12. 发版前按 `translation-mod/docs/RELEASE_CHECKLIST.md` 检查 Release 包。

### 翻译规则

应该翻译：

- 菜单、按钮、标题、标签和设置项。
- 零件分类、零件名称、零件属性和固定悬浮说明。
- 固定提示、固定错误信息、教程文本和上传发布流程。

谨慎翻译：

- `Fly`、`Open`、`Public`、`Basic`、`Light` 这类短词。优先使用 `contextEntries`。
- 梗、双关和很难直译的文本。必要时使用低可视度译者注。
- 输入信号名。当前约定为英文优先、中文括注，例如 `Yaw（偏航）`；`VTOL` 保持英文。

不要翻译：

- 玩家作品名、存档名、飞机名、服务器名和用户名。
- 零件实例名或可编辑输入框里的自定义名称。
- 坐标、重量、容量、速度、百分比、颜色编号和运行时数值。
- 临时调试文本或不能确定语境的短词。

### 翻译数据格式

普通固定文本：

```json
{
  "entries": [
    {
      "key": "New Craft",
      "value": "新建作品"
    }
  ]
}
```

同词不同义时使用上下文：

```json
{
  "contextEntries": [
    {
      "key": "Wheel",
      "value": "方向盘",
      "sceneName": "Designer",
      "gameObjectPathContains": "/PartProperties_ControlBaseData/Preset/"
    }
  ]
}
```

动态后缀文本：

```json
{
  "dynamicSuffixEntries": [
    {
      "sourceSuffix": "% Favor Torque",
      "valueSuffix": "% 倾向扭矩"
    }
  ]
}
```

上面的规则可以把 `25% Favor Torque` 翻译为 `25% 倾向扭矩`，不需要为每个百分比写固定条目。

### 发布

仓库提供 GitHub Actions 发版流程，但编译依赖本机游戏程序集：

```text
SimplePlanes 2_Data\Managed
```

因此自动发版需要 Windows self-hosted runner，且 runner 上必须安装游戏或具备等效的合法本地依赖。

常规发版：

```powershell
git tag v0.1.0
git push origin v0.1.0
```

也可以在 GitHub Actions 页面手动运行 `Build release packages`。
