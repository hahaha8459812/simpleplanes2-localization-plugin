# Release Checklist

[中文](RELEASE_CHECKLIST.md)

## Before Build

- Confirm `content/translations/zh-CN.fragments/*.json` contains the latest translation source.
- Confirm capture files, screenshots, and temporary notes are not included in the package.
- Confirm `content/settings.release.json` uses `translate` mode.
- Confirm `content/settings.dev.json` uses `collect` mode.
- Confirm the plugin version, `CHANGELOG.md`, and `RELEASE_NOTES.md` are updated.
- If C# source changed, close the game before replacing the DLL.

## Build

```powershell
.\build.ps1
```

Expected output:

- `release/SimplePlanes2TranslationMod-Dev.zip`
- `release/SimplePlanes2TranslationMod-Release.zip`
- repository root `index.json`

## Release Package Test

1. Extract `SimplePlanes2TranslationMod-Release.zip`.
2. Confirm the zip root directly contains `mod.json` and `BepInEx`.
3. Confirm the zip does not contain bundled BepInEx files such as `.doorstop_version`, `doorstop_config.ini`, `winhttp.dll`, or `BepInEx/core/BepInEx.dll`.
4. Confirm the plugin DLL is located at `BepInEx/plugins/SimplePlanes2Translation/SimplePlanes2Translation.dll`.
5. Confirm `mod.json` has the correct `version`, `fileName`, `entryDll`, and `pluginDirectory`.
6. Confirm repository root `index.json` points `version`, `fileName`, and `downloadUrl` at this release.
7. In a game directory that already has BepInEx 5 installed, copy all zip contents into the folder containing `SimplePlanes 2.exe`, or install it with `simpleplanes2-mod-manager`.
8. Start the game and open the main menu and designer.
9. Confirm Chinese text does not render as boxes.
10. Confirm `F2` reloads translations.
11. Confirm `F1` toggles localization.
12. Check the part list, tooltips, upload page, and settings page for obvious untranslated text.

## Dev Package Test

1. Install `SimplePlanes2TranslationMod-Dev`.
2. Open target pages in game.
3. Exit the game.
4. Confirm `captured-texts.json` is created or updated.
5. Confirm captured entries include object paths and parent paths.

## Distribution

Give regular players only:

```text
SimplePlanes2TranslationMod-Release.zip
```

Give translation contributors the dev package only when they need to collect text.

Regular plugin release packages do not bundle BepInEx. BepInEx is installed and maintained by `simpleplanes2-mod-manager` or by the player separately.

## GitHub Actions Release

The repository includes `.github/workflows/release.yml`.

Compilation requires game assemblies from `SimplePlanes 2_Data\Managed`, so the release runner must be a Windows self-hosted runner with the game installed, or an equivalent legally prepared Windows environment.

Before release:

- Confirm the self-hosted runner is online.
- Confirm `SimplePlanes 2` is installed on the runner.
- If the game is not in the default path, set repository variable `SP2_GAME_DIR`.
- Use a `v*` tag, such as `v0.1.0`.

Tag release example:

```powershell
git tag v0.1.0
git push origin v0.1.0
```

You can also run `Build release packages` manually in GitHub Actions and fill `release_tag`.
