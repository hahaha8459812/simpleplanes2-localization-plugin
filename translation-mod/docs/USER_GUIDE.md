# 玩家使用说明

这份说明面向只想安装和使用汉化 Mod 的玩家，不需要本地编译，也不需要理解翻译采集流程。

## 下载哪个文件

普通玩家下载：

```text
SimplePlanes2TranslationMod-Release.zip
```

不要下载 `Dev` 包。`Dev` 包用于翻译采集，会生成额外的文本记录文件。

## 安装

1. 关闭 `SimplePlanes 2`。
2. 解压 `SimplePlanes2TranslationMod-Release.zip`。
3. 在解压后的文件夹里右键运行 `install.ps1`。
4. 如果 PowerShell 阻止脚本运行，在该文件夹空白处打开 PowerShell，执行：

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\install.ps1
```

如果游戏不在默认 Steam 路径，指定游戏目录：

```powershell
.\install.ps1 -GameDir "D:\SteamLibrary\steamapps\common\SimplePlanes 2"
```

安装成功后，插件会被复制到：

```text
SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation
```

## 使用

启动游戏即可自动汉化。

快捷键：

- `F6`：重新加载汉化配置和词表。
- `F10`：临时开关汉化，方便对照原文和截图反馈。

正常游玩不需要打开任何额外程序。

## 卸载

关闭游戏后，删除：

```text
SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation
```

如果你只为了这个汉化安装了 BepInEx，也可以删除游戏目录下的这些文件和目录：

```text
BepInEx
doorstop_config.ini
winhttp.dll
```

如果你还装了其他 BepInEx Mod，不要删除整个 `BepInEx` 目录，只删除本汉化插件目录。

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

首次加载 BepInEx 和字体时可能会慢一些。后续启动通常会更稳定。

### F10 没反应

确认当前安装的是较新的 Release 包。旧版本使用过其他快捷键。

### 安装脚本提示找不到游戏

用 `-GameDir` 手动指定 `SimplePlanes 2.exe` 所在目录，不要指定到 `SimplePlanes 2_Data`。
