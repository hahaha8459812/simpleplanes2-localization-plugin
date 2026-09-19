# v0.1.8

本次更新修复上一版关闭周期扫描后出现的漏翻：零件列表等随预制体实例化出现的界面，其文本在激活时并不经过赋值，需要新的激活时钩子。

## 主要变化

- 新增激活时翻译钩子：`TextMeshProUGUI.OnEnable`、`TextMeshPro.OnEnable` 与 `UnityEngine.UI.Text.OnEnable`。
- 补齐 `TMP_Text.SetText` 的 `char[]`、`StringBuilder` 与数值格式化重载。
- 上下文信息改为按需构造，降低每帧刷新文本的开销。

# v0.1.7

本次更新把翻译时机从“周期性扫描界面”改为“文本赋值时翻译”，并收敛了字体处理与字典查表的开销。

## 主要变化

- 新增赋值钩子，覆盖 `TMP_Text.text`、`TMP_Text.SetText` 与 `UnityEngine.UI.Text.text`，界面文本在写入时即完成翻译。
- 周期性场景扫描默认关闭（`EnableSceneScan` 默认 `false`），仅在场景加载后补扫一次；需要旧行为时可在设置中打开。
- 字体回退只注入一次，取消每条文本一次的全量字体查找与强制网格重建。
- 字典查表加入记忆化与按场景索引的上下文条目。
- 含中文的文本不再翻译，也不再写入缺失文本清单与采集结果。
- 输入框内的用户输入不再被翻译。
- 发行包改为纯手动部署结构：不再包含 `mod.json`，zip 内只有 `BepInEx/plugins/SimplePlanes2Translation/`，解压到游戏根目录即可。

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
