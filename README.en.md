# SimplePlanes 2 Localization Plugin

[中文](README.md)

A runtime localization plugin for `SimplePlanes 2`. The current package focuses on Simplified Chinese localization for the main menu, designer UI, part list, part properties, tooltips, settings, upload flow, and selected flight UI.

The plugin uses `BepInEx + Harmony`. Release packages contain only the plugin and required resources and do not bundle BepInEx; install by extracting the zip manually.

## Player Guide

### Download

Regular players should download this file from GitHub Releases:

```text
SimplePlanes2TranslationMod-Release.zip
```

Do not download the `Dev` package unless you are helping with text collection. The dev package writes extra capture files.

### Installing BepInEx

The release package does not bundle BepInEx. If you already have BepInEx 5 set up, skip to "Installing this plugin".

This plugin is tested on BepInEx 5.4.23.5 (Mono x64); use a [BepInEx 5.4.x build](https://github.com/BepInEx/BepInEx/releases). Download [BepInEx_win_x64_5.4.23.5.zip](https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.5/BepInEx_win_x64_5.4.23.5.zip) and extract it into the game root, the folder containing `SimplePlanes 2.exe`:

```text
SimplePlanes 2\
├─ winhttp.dll
├─ doorstop_config.ini
├─ .doorstop_version
└─ BepInEx\
```

Launch the game once and quit; `BepInEx\plugins\` and `BepInEx\config\` are created automatically.

### Installing this plugin

1. Close `SimplePlanes 2`.
2. Extract `SimplePlanes2TranslationMod-Release.zip`.
3. Put the extracted `BepInEx` folder into the game root.
4. Start the game.

After installation, the plugin is placed under:

```text
SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation
```

### Usage

Start the game. The localization loads automatically.

Hotkeys:

- `F1`: temporarily toggle localization on or off.
- `F2`: reload localization settings and the translation catalog.

Normal gameplay does not require any extra program.

### Uninstall

Close the game, then delete:

```text
SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation
```

If BepInEx was installed only for this localization plugin, manually delete:

```text
BepInEx
.doorstop_version
changelog.txt
doorstop_config.ini
winhttp.dll
```

If you use other BepInEx mods, only remove this plugin directory.

### FAQ

#### Some text is still English

Press `F2` once. If the text is still English, it probably has not been translated yet.

#### Chinese text appears as boxes

Check that this font file exists:

```text
BepInEx\plugins\SimplePlanes2Translation\fonts\SourceHanSansSC-Regular.otf
```

If it is missing, reinstall the release package.

#### The game starts slowly

The first startup with BepInEx and the bundled Chinese font can be slower. Later launches are usually more stable.

#### Hotkeys do not work

Confirm you installed a recent release package. Older builds used `F6/F10`, `Alt + +/-`, or other test hotkeys.

## Reference

- [Development and build](translation-mod/docs/DEVELOPMENT.en.md): project layout, build, runtime modes, release.
- [Translation workflow](translation-mod/docs/TRANSLATION_WORKFLOW.en.md): workflow, rules, data format, and extractable text sources.
- [Release checklist](translation-mod/docs/RELEASE_CHECKLIST.en.md).
