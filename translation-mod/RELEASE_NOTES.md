# v0.1.6

本次更新主要调整并完善发行结构，使插件包可以被 `simpleplanes2-mod-manager` 直接识别、安装和后续更新。

## 发行结构调整

- 普通发行包不再内置 BepInEx。
- Release zip 根目录新增 `mod.json`。
- 仓库根目录新增并由构建脚本同步更新 `index.json`。
- `mod.json` 与 `index.json` 使用无 BOM UTF-8，降低远程 JSON 解析和第三方工具读取时的兼容风险。
- 插件文件按 BepInEx 目录结构放置：

```text
BepInEx/plugins/SimplePlanes2Translation/SimplePlanes2Translation.dll
```

## 管理器兼容

- `mod.json` 包含插件 id、显示名称、版本、简介、包文件名、入口 DLL、插件目录和配置文件路径。
- `index.json` 指向当前版本的 GitHub Release 下载地址，可供插件管理器从仓库 URL 或 `index.json` URL 安装。
- Release 包只包含插件本体、翻译词表、中文字体、设置文件和 `mod.json`。

## 使用说明

- 使用插件管理器安装时，直接选择本 Release 的 `SimplePlanes2TranslationMod-Release.zip`，或输入仓库地址：

```text
https://github.com/hahaha8459812/simpleplanes2-localization-plugin
```

- 手动安装时，需要先安装 BepInEx 5 Mono x64，然后把 zip 内容解压到 `SimplePlanes 2.exe` 所在目录。
