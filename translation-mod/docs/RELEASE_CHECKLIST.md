# 发布检查清单

## 构建前

- 确认 `content/translations/zh-CN.fragments/*.json` 是最新翻译源。
- 确认没有把 `captured-texts.json`、`missing-texts.txt`、截图或临时记录放进分发包。
- 确认 `content/settings.release.json` 使用 `translate` 模式。
- 确认 `content/settings.dev.json` 使用 `collect` 模式。
- 如果改过 C#，确认游戏已退出，方便覆盖 DLL。

## 构建

```powershell
.\build.ps1
```

构建成功后应存在：

- `release/SimplePlanes2TranslationMod-Dev.zip`
- `release/SimplePlanes2TranslationMod-Release.zip`

## 安装验证

1. 解压 `SimplePlanes2TranslationMod-Release.zip`。
2. 运行 `install.ps1`。
3. 启动游戏进入主菜单和设计器。
4. 检查中文字体没有方框。
5. 检查 `F6` 可以热重载。
6. 检查 `F10` 可以开关翻译。
7. 检查零件列表、悬浮说明、上传页面和设置页面没有明显英文残留。

## 开发包验证

1. 安装 `SimplePlanes2TranslationMod-Dev`。
2. 进入游戏走一遍目标页面。
3. 退出游戏。
4. 确认插件目录生成或更新 `captured-texts.json`。
5. 确认采集内容包含对象路径和父节点路径。

## 分发说明

给普通玩家只发 `SimplePlanes2TranslationMod-Release.zip`。

给翻译协作者可以发 `SimplePlanes2TranslationMod-Dev.zip`，但要说明它会生成采集文件。
