# 翻译维护流程

[English](TRANSLATION_WORKFLOW.en.md)

面向翻译维护者。玩家安装与使用见[根目录 README](../../README.md)。

## 标准翻译流程

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
12. 发版前按 [RELEASE_CHECKLIST.md](RELEASE_CHECKLIST.md) 检查 Release 包。

## 翻译规则

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

## 翻译数据格式

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

## 文本来源（可静态提取）

除游玩采集外，大部分界面文本可以直接从游戏文件提取。静态提取适合盘点“一共存在哪些文本、字典漏了哪些”，运行期采集仍然是判断实际显示形态与语境的依据，两者互补。

| 来源 | 可提取内容 | 说明 |
|---|---|---|
| `%USERPROFILE%\AppData\LocalLow\Jundroo\SimplePlanes 2\DesignerParts.xml` | 零件的 `name`、`category`、`header`、`description` | 游戏运行时通过 `Game.GetPathForDocument` 读取。本机 0.7.8 为 188 个零件；`description` 是选中零件后显示的说明段落 |
| 同目录 `AircraftThemes.xml` | 涂装主题的 `name` | 本机 10 个 |
| 同目录 `Levels.xml` | 关卡的 `name`、`category`、`description` | 本机 1 个（`Sandbox`） |
| 同目录 `ControlInputData.xml` | Rewired 输入配置中的名称 | 按翻译规则，输入信号名英文优先、中文括注 |
| `SimplePlanes 2_Data\Managed` 下的 `Game.dll`、`Jundroo.Common.dll`、`Jundroo.Packages.dll` | 代码内硬编码文案（IL 中的 `ldstr`） | 用 `monodis` 或 `ikdasm` 转 IL 后提取。`Game.dll` 约 8,500 条唯一字符串，其中一部分是 UI 文案 |
| `SimplePlanes 2_Data\resources.assets` 等资源文件 | 资源内 XML 的节点文本与属性值 | 例如 `<Text text="FLY SOLO" />`、`<Style ... text="...">`。按属性值和节点文本取值，不要按“最长可打印串”取值，否则会把整块 XML 当成一个字符串 |

注意：LocalLow 下的 XML 由游戏维护，更新或重置时可能被重写，因此静态提取只作为盘点手段，插件运行时不依赖这些文件。
