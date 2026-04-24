# SimplePlanes 2 Localization Plugin

[中文](README.md)

This is a runtime localization plugin for `SimplePlanes 2`. The current package focuses on Simplified Chinese localization for the main menu, designer UI, part list, part properties, tooltips, upload pages, and related game UI.

The plugin uses `BepInEx + Harmony`. Players only need to install the release package and start the game; no local build is required.

## Player Guide

### Download

Regular players should download:

```text
SimplePlanes2TranslationMod-Release.zip
```

Do not download the `Dev` package unless you are helping with translation collection. The dev package writes extra text capture files.

### Install

1. Close `SimplePlanes 2`.
2. Extract `SimplePlanes2TranslationMod-Release.zip`.
3. Put all extracted contents into the `SimplePlanes 2` game root, the same folder that contains `SimplePlanes 2.exe`.
4. Start the game.

The release package already uses the game root layout, so the easiest path is to extract the zip contents directly into the local game files folder.

If you prefer the installer script, run:

```powershell
.\install.ps1
```

If the game is installed somewhere else, pass the game directory manually:

```powershell
.\install.ps1 -GameDir "D:\SteamLibrary\steamapps\common\SimplePlanes 2"
```

If PowerShell blocks the script, run this first:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
```

After installation, the plugin is placed under:

```text
SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation
```

### Usage

Start the game. The localization loads automatically.

Hotkeys:

- `F6`: reload localization settings and translations.
- `F10`: temporarily toggle localization on or off.

### Uninstall

Close the game, then delete:

```text
SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation
```

If BepInEx was installed only for this localization plugin, you may also remove:

```text
BepInEx
.doorstop_version
changelog.txt
doorstop_config.ini
winhttp.dll
```

If you use other BepInEx mods, only remove this plugin directory.

## FAQ

### Some text is still English

Press `F6` once. If the text is still English, it probably has not been translated yet.

### Chinese text appears as boxes

Check that this font file exists:

```text
BepInEx\plugins\SimplePlanes2Translation\fonts\SourceHanSansSC-Regular.otf
```

If it is missing, reinstall the release package.

### The game starts slowly

The first startup with BepInEx and the bundled Chinese font can be slower. Later launches are usually more stable.

### The installer cannot find the game

Use `-GameDir` and point it to the folder containing `SimplePlanes 2.exe`, not `SimplePlanes 2_Data`.

## Project Docs

- [Project README](translation-mod/README.en.md)
- [Translation workflow](translation-mod/docs/TRANSLATION_WORKFLOW.en.md)
- [Release checklist](translation-mod/docs/RELEASE_CHECKLIST.en.md)

## Development

Source and build scripts are under:

```text
translation-mod/
```

Build locally:

```powershell
cd translation-mod
.\build.ps1
```

Packages are written to:

```text
translation-mod\release
```

The repository includes a GitHub Actions release workflow, but compilation requires the local `SimplePlanes 2_Data\Managed` assemblies from an installed copy of the game. Use a Windows self-hosted runner.
