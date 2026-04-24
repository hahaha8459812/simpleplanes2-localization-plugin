# Translation Workflow

[中文](TRANSLATION_WORKFLOW.md)

## Modes

- `collect`: collect text only, without translating it.
- `translate`: translate text only, without recording missing text.
- `hybrid`: translate and collect at the same time.

Press `F6` in game to reload `settings.json` and the current translation catalog.

## Text Collection

For development, `hybrid` is usually the most useful mode:

```json
{
  "Mode": "hybrid",
  "Language": "zh-CN",
  "EnableSceneScan": true,
  "LogMissingTexts": true,
  "CaptureStandaloneTmpTexts": true,
  "CapturedTextsFileName": "captured-texts.json",
  "CaptureFlushIntervalSeconds": 10.0,
  "SceneScanIntervalSeconds": 0.1,
  "ApplyBoldStyleToTranslatedText": true,
  "VerboseLogging": false
}
```

After collection, check:

```text
E:\Game\steam\steamapps\common\SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation\captured-texts.json
E:\Game\steam\steamapps\common\SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation\missing-texts.txt
```

## What To Translate

Translate:

- Menus, buttons, headings, and labels.
- Part categories, part names, and part tooltips.
- Fixed notices, fixed error messages, and settings fields.

Be careful with:

- Short repeated words, such as `FLY`, `Public`, or `Open`. Prefer `contextEntries`.
- Jokes or puns. Use a low-visibility translator note when helpful.

Do not translate:

- User craft names, save names, and aircraft names.
- Part instance names, such as `Fuselage #72`.
- Weight, capacity, coordinates, percentages, and color identifiers.
- Runtime-generated debug or status values.

## Editing Translations

Only edit:

```text
content/translations/zh-CN.fragments/*.json
```

Do not edit:

```text
content/translations/zh-CN.json
```

The build script regenerates the merged catalog.

## Hot Update

Pure translation changes do not require restarting the game:

```powershell
.\build.ps1
Copy-Item .\content\translations\zh-CN.json "E:\Game\steam\steamapps\common\SimplePlanes 2\BepInEx\plugins\SimplePlanes2Translation\translations\zh-CN.json" -Force
```

Then press `F6` in game.

## When Restart Is Required

Restart the game after:

- Editing `src/*.cs`.
- Replacing the DLL.
- Updating BepInEx or core dependencies.
- Changing font loading behavior.

The DLL cannot be overwritten while the game is running.
