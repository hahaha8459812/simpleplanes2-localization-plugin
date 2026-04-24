# 翻译维护流程

[English](TRANSLATION_WORKFLOW.en.md)

## 模式

- `collect`：只采集文本，不翻译。适合第一次粗采集。
- `translate`：只翻译，不记录缺失文本。适合普通玩家。
- `hybrid`：一边翻译一边采集。适合边玩边补漏。

游戏内按 `F6` 会重载 `settings.json` 和当前语言词表。

## 文本采集

开发时优先使用 `hybrid`：

```json
{
  "Mode": "hybrid",
  "Language": "zh-CN",
  "EnableSceneScan": true,
  "LogMissingTexts": true,
  "CaptureStandaloneTmpTexts": true,
  "CapturedTextsFileName": "captured-texts.json",
  "CaptureFlushIntervalSeconds": 10.0,
  "SceneScanIntervalSeconds": 0.1,
  "ApplyBoldStyleToTranslatedText": true,
  "VerboseLogging": false
}
```

采集完成后查看：

```text
E:\Game\steam\steamapps\common\SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation\captured-texts.json
E:\Game\steam\steamapps\common\SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation\missing-texts.txt
```

## 判断哪些文本要翻译

应该翻译：

- 菜单、按钮、标题、标签。
- 零件分类、零件短名、零件悬浮说明。
- 固定提示、固定错误信息和设置项。

谨慎翻译：

- 同名短词，例如 `FLY`、`Public`、`Open`。这类优先使用 `contextEntries`。
- 有双关或梗的文本。可以加低可视度译者注。

不要翻译：

- 用户作品名、存档名、飞机名。
- 零件实例名，例如 `Fuselage #72`。
- 重量、容量、坐标、百分比和颜色编号。
- 运行时生成的调试或状态数值。

## 修改翻译

只编辑：

```text
content/translations/zh-CN.fragments/*.json
```

不要直接编辑：

```text
content/translations/zh-CN.json
```

构建脚本会重新生成合并词表。

## 热更新

纯文本修改不需要重启游戏：

```powershell
.\build.ps1
Copy-Item .\content\translations\zh-CN.json "E:\Game\steam\steamapps\common\SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation\translations\zh-CN.json" -Force
```

然后在游戏里按 `F6`。

## 需要重启游戏的情况

- 修改 `src/*.cs`。
- 更新 DLL。
- 更新 BepInEx 或核心依赖。
- 字体加载逻辑发生变化。

DLL 被游戏加载时无法覆盖。需要先退出游戏，再安装新 DLL。
