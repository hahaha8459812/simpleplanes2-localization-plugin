# Player Guide

[中文](USER_GUIDE.md)

This guide is for players who only want to install and use the Chinese translation mod. You do not need to build the project locally.

## Download

Regular players should download:

```text
SimplePlanes2TranslationMod-Release.zip
```

Do not download the `Dev` package unless you are helping with translation text collection. The dev package writes extra capture files.

## Install

1. Close `SimplePlanes 2`.
2. Extract `SimplePlanes2TranslationMod-Release.zip`.
3. Run `install.ps1` in the extracted folder.
4. Start the game.

If PowerShell blocks the script, open PowerShell in the extracted folder and run:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\install.ps1
```

If the game is installed somewhere else, pass the game directory manually:

```powershell
.\install.ps1 -GameDir "D:\SteamLibrary\steamapps\common\SimplePlanes 2"
```

After installation, the plugin is placed under:

```text
SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation
```

## Usage

Start the game. The localization loads automatically.

Hotkeys:

- `F6`: reload localization settings and translations.
- `F10`: temporarily toggle localization on or off.

## Uninstall

Close the game, then delete:

```text
SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation
```

If BepInEx was installed only for this localization plugin, you may also remove:

```text
BepInEx
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
