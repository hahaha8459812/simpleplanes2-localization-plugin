# Changelog

## 2026-04-25

- 搭建 `BepInEx + Harmony` 运行时翻译 Mod。
- 支持 `collect`、`translate`、`hybrid` 模式。
- 增加 `F6` 热重载和 `F10` 翻译开关。
- 增加文本采集上下文：对象路径、父节点路径、组件类型、兄弟序号和锚点位置。
- 内置 `Source Han Sans SC Regular`，解决中文方框和混合字体问题。
- 翻译设计器主要菜单、零件列表、零件属性、悬浮说明、上传发布页和部分设置页。
- 将翻译源拆分到 `zh-CN.fragments`，构建时合并为 `zh-CN.json`。
- 生成 `Dev` 与 `Release` 两种分发包，均带一键安装脚本。
