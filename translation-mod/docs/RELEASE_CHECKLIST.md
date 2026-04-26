# 发布检查清单

[English](RELEASE_CHECKLIST.en.md)

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
2. 确认压缩包根目录直接包含 `BepInEx`、`.doorstop_version`、`doorstop_config.ini`、`winhttp.dll`。
3. 把压缩包全部内容复制到 `SimplePlanes 2.exe` 所在目录。
4. 也可以运行 `install.ps1` 验证备用安装方式。
5. 启动游戏进入主菜单和设计器。
6. 检查中文字体没有方框。
7. 检查 `F2` 可以热重载。
8. 检查 `F1` 可以开关翻译。
9. 检查零件列表、悬浮说明、上传页面和设置页面没有明显英文残留。

## 开发包验证

1. 安装 `SimplePlanes2TranslationMod-Dev`。
2. 进入游戏走一遍目标页面。
3. 退出游戏。
4. 确认插件目录生成或更新 `captured-texts.json`。
5. 确认采集内容包含对象路径和父节点路径。

## 分发说明

给普通玩家只发 `SimplePlanes2TranslationMod-Release.zip`。

给翻译协作者可以发 `SimplePlanes2TranslationMod-Dev.zip`，但要说明它会生成采集文件。

## GitHub Actions 发布

仓库内置 `.github/workflows/release.yml`。

由于当前插件编译需要 `SimplePlanes 2_Data\Managed` 下的游戏程序集，发布 runner 必须是装有游戏的 Windows self-hosted runner，或其他拥有同等合法本地依赖的 Windows runner。

发布前确认：

- self-hosted runner 在线。
- runner 上已安装 `SimplePlanes 2`。
- 如果游戏不在默认路径，仓库变量 `SP2_GAME_DIR` 已设置正确。
- tag 使用 `v*` 格式，例如 `v0.1.0`。

发布命令示例：

```powershell
git tag v0.1.0
git push origin v0.1.0
```

手动发布也可以在 GitHub Actions 页面运行 `Build release packages`，填写 `release_tag`。
