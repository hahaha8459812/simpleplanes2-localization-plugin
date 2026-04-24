# zh-CN 翻译分片

这里的 JSON 文件是中文翻译的源文件。`../zh-CN.json` 是构建产物，会被 `build.ps1` 自动覆盖，不要手动维护。

## 文件分工

- `00-common-and-startup.json`：通用词和启动相关文本。
- `10-main-menu.json`：主菜单。
- `20-designer-browser-and-selection.json`：设计器浏览、选择、基础入口。
- `21-designer-shape-and-wing-editors.json`：机身、机翼和形状编辑。
- `22-designer-part-properties.json`：零件属性标题和字段。
- `23-designer-environment-and-analysis.json`：设计器环境、分析工具。
- `24-designer-menu-and-tutorials.json`：设计器菜单和教程。
- `25-designer-paint-and-appearance.json`：喷涂、外观和材质。
- `26-designer-transform-and-connections.json`：移动、旋转、连接和镜像。
- `27-designer-part-list.json`：零件列表分类。
- `28-designer-tooltips-and-search.json`：悬浮说明、搜索结果和较长说明。
- `29-designer-part-names.json`：零件短名。
- `30-upload-and-sharing.json`：上传、分享和公开状态。
- `31-save-load-and-validation.json`：保存、加载、合法性校验。
- `32-settings-dialog.json`：设置窗口。
- `99-misc.json`：暂时无法归类但需要保留的文本。

## 格式

普通精确匹配：

```json
{
  "key": "Wheels",
  "value": "车轮"
}
```

按上下文匹配：

```json
{
  "key": "FLY",
  "value": "起飞",
  "sceneName": "Designer",
  "gameObjectPathContains": "/flyout-menu/"
}
```

`contextEntries` 适合处理同一个英文词在不同 UI 位置需要不同翻译的情况。

## 维护规则

- 优先补到最具体的页面分片。
- 零件短名放进 `29-designer-part-names.json`。
- 零件悬浮说明放进 `28-designer-tooltips-and-search.json`。
- 分类标题放进 `27-designer-part-list.json`。
- 不要把动态文本、用户作品名、存档名、数值和实例编号放进词表。
- 术语改动同步更新 `TERMS.md`。
