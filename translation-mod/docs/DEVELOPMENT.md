# 开发与构建

[English](DEVELOPMENT.en.md)

面向开发者与维护者。玩家安装与使用见[根目录 README](../../README.md)。

## 项目结构

```text
translation-mod/
  src/                                  # BepInEx/Harmony 插件源码
  content/settings.release.json          # 分发版设置，默认 translate
  content/settings.dev.json              # 开发采集版设置，默认 collect
  content/translations/zh-CN.fragments/  # 翻译源文件，按页面和功能拆分
  content/translations/zh-CN.json        # 构建生成的合并词表，不要手工编辑
  docs/                                  # 开发、翻译维护与发布文档
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

## 构建

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

Release zip 内只有 `BepInEx/plugins/SimplePlanes2Translation/` 一层，解压到游戏根目录即可：

```text
BepInEx/plugins/SimplePlanes2Translation/SimplePlanes2Translation.dll
BepInEx/plugins/SimplePlanes2Translation/settings.json
BepInEx/plugins/SimplePlanes2Translation/translations/zh-CN.json
BepInEx/plugins/SimplePlanes2Translation/fonts/
```

发行包不内置 BepInEx，需要玩家自行安装 BepInEx 5 Mono x64。

如果只修改翻译文本，可以构建后把新的 `zh-CN.json` 复制到游戏插件目录，再在游戏内按 `F2` 热重载。

如果修改了 C# 代码，需要重新构建 DLL，并在游戏退出后覆盖 DLL，再重新启动游戏。

## 运行模式

插件设置位于：

```text
SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation\settings.json
```

常用模式：

- `translate`：只翻译，不记录缺失文本。普通玩家使用。
- `collect`：只采集文本，不翻译。适合第一次粗采集。
- `hybrid`：一边翻译一边采集。适合边玩边补漏。

开发时常用 `hybrid`，因为它能保留现有汉化效果，同时记录新出现的文本。

## 发布

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
