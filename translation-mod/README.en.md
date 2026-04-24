# SimplePlanes 2 Chinese Translation Mod

[中文](README.md)

This is a Simplified Chinese translation mod for `SimplePlanes 2`. It uses `BepInEx + Harmony` to translate Unity UI text at runtime through a local JSON translation catalog.

The current goal is to localize the main UI, designer, part list, part properties, tooltips, upload flow, and related pages to a playable and maintainable level.

## Player Guide

### Download

Regular players should download:

```text
SimplePlanes2TranslationMod-Release.zip
```

Do not download `SimplePlanes2TranslationMod-Dev.zip` unless you are helping collect translation text. The dev package writes extra capture files.

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

If your game is installed elsewhere:

```powershell
.\install.ps1 -GameDir "D:\SteamLibrary\steamapps\common\SimplePlanes 2"
```

If PowerShell blocks the script, run this first:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
```

The plugin is installed to:

```text
BepInEx\plugins\SimplePlanes2Translation
```

### Usage

Start the game. The translation loads automatically.

Hotkeys:

- `F6`: reload localization settings and translation catalog.
- `F10`: temporarily toggle localization on or off.

### Uninstall

Close the game, then delete:

```text
SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation
```

If BepInEx was installed only for this translation mod, you can also remove:

```text
BepInEx
.doorstop_version
changelog.txt
doorstop_config.ini
winhttp.dll
```

If you use other BepInEx mods, only remove this plugin directory.

### FAQ

Some text is still English: press `F6` once. If it is still English, that text probably has not been translated yet.

Chinese text appears as boxes: check that `fonts\SourceHanSansSC-Regular.otf` exists in the plugin directory. Reinstall the release package if it is missing.

The game starts slowly: the first startup with BepInEx and the bundled Chinese font can be slower.

Installer cannot find the game: pass `-GameDir` and point it to the folder containing `SimplePlanes 2.exe`, not `SimplePlanes 2_Data`.

## Features

- Runtime translation for `TextWidget.SetText()` and standalone `TextMeshProUGUI` text.
- `collect`, `translate`, and `hybrid` modes.
- Development capture includes object paths, parent paths, sibling index, and anchored position.
- Bundled Source Han Sans SC font for reliable Chinese rendering.
- `F6` hot reload.
- `F10` translation toggle.
- Single-folder release package with installer script.

## Build

Default game path:

```powershell
E:\Game\steam\steamapps\common\SimplePlanes 2
```

Build:

```powershell
.\build.ps1
```

Custom game path:

```powershell
.\build.ps1 -GameDir "D:\SteamLibrary\steamapps\common\SimplePlanes 2"
```

Generated packages:

- `release/SimplePlanes2TranslationMod-Dev.zip`
- `release/SimplePlanes2TranslationMod-Release.zip`

## GitHub Actions Releases

The repository includes `.github/workflows/release.yml`.

The plugin currently references game assemblies from `SimplePlanes 2_Data\Managed`, so normal GitHub-hosted runners cannot compile it directly. Use a Windows self-hosted runner with the game installed.

If the game path differs on the runner, set repository variable:

```text
SP2_GAME_DIR=D:\SteamLibrary\steamapps\common\SimplePlanes 2
```

Release options:

- Push a `v*` tag, such as `v0.1.0`.
- Manually run `Build release packages` in GitHub Actions.
