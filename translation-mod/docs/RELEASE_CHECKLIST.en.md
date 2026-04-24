# Release Checklist

[中文](RELEASE_CHECKLIST.md)

## Before Build

- Confirm `content/translations/zh-CN.fragments/*.json` contains the latest translation source.
- Confirm capture files, screenshots, and temporary notes are not included in the package.
- Confirm `content/settings.release.json` uses `translate` mode.
- Confirm `content/settings.dev.json` uses `collect` mode.
- If C# source changed, close the game before replacing the DLL.

## Build

```powershell
.\build.ps1
```

Expected output:

- `release/SimplePlanes2TranslationMod-Dev.zip`
- `release/SimplePlanes2TranslationMod-Release.zip`

## Release Package Test

1. Extract `SimplePlanes2TranslationMod-Release.zip`.
2. Run `install.ps1`.
3. Start the game and open the main menu and designer.
4. Confirm Chinese text does not render as boxes.
5. Confirm `F6` reloads translations.
6. Confirm `F10` toggles localization.
7. Check the part list, tooltips, upload page, and settings page for obvious untranslated text.

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
