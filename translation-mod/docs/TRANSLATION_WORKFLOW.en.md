# Translation Workflow

[中文](TRANSLATION_WORKFLOW.md)

For translation maintainers. For player installation and usage see the [root README](../../README.en.md).

## Standard Translation Workflow

1. Use dev settings, usually `hybrid` mode, with `LogMissingTexts` and `CaptureStandaloneTmpTexts` enabled.
2. Clear or archive old `captured-texts.json` and `missing-texts.txt` before collecting a new page or feature area.
3. Start the game, press `F2` to reload settings, then walk through the target UI, hover tooltips, dropdown options, and related property pages.
4. Exit the game or wait for capture files to flush, then inspect `captured-texts.json` and `missing-texts.txt` in the plugin directory.
5. Use object paths, parent paths, scene names, and UI positions to understand context. Organize by page logic instead of only by English source text.
6. Choose the right rule type: fixed UI text uses `entries`; ambiguous repeated words use `contextEntries`; stable numeric templates use `dynamicSuffixEntries` or `dynamicPrefixEntries`.
7. Skip craft names, save names, part instance names, server names, tags, coordinates, weight, capacity, percentages, color IDs, and runtime-only values.
8. Add translations to the matching `zh-CN.fragments/*.json` file. Keep terminology consistent with existing translations.
9. Run `.\build.ps1` to validate duplicate keys and regenerate `zh-CN.json`.
10. Copy the new `zh-CN.json` into the game plugin directory and press `F2` to verify. If the DLL changed, close the game, replace the DLL, and restart.
11. After verification, switch runtime mode back to `translate` and disable missing-text logging before distribution.
12. Before release, run through [RELEASE_CHECKLIST.en.md](RELEASE_CHECKLIST.en.md).

## Translation Rules

Translate:

- Menus, buttons, headings, labels, and settings.
- Part categories, part names, part properties, and fixed tooltips.
- Fixed notices, fixed errors, tutorials, and upload flow text.

Be careful with:

- Short repeated words such as `Fly`, `Open`, `Public`, `Basic`, and `Light`. Prefer `contextEntries`.
- Jokes, puns, and text that needs a low-visibility translator note.
- Input signal names. Current convention is English-first with Chinese notes, such as `Yaw（偏航）`; `VTOL` stays English.

Do not translate:

- Player craft names, save names, aircraft names, server names, or usernames.
- Part instance names or editable custom names.
- Coordinates, weight, capacity, speed, percentages, color IDs, or runtime numeric values.
- Temporary debug text or short words without reliable context.

## Translation Data Format

Fixed text:

```json
{
  "entries": [
    {
      "key": "New Craft",
      "value": "新建作品"
    }
  ]
}
```

Context-sensitive text:

```json
{
  "contextEntries": [
    {
      "key": "Wheel",
      "value": "方向盘",
      "sceneName": "Designer",
      "gameObjectPathContains": "/PartProperties_ControlBaseData/Preset/"
    }
  ]
}
```

Dynamic suffix text:

```json
{
  "dynamicSuffixEntries": [
    {
      "sourceSuffix": "% Favor Torque",
      "valueSuffix": "% 倾向扭矩"
    }
  ]
}
```

This translates `25% Favor Torque` into `25% 倾向扭矩` without adding every percentage as a fixed entry.

## Text Sources (Static Extraction)

Besides play-session capture, most UI text can be extracted directly from game files. Static extraction is the way to inventory what text exists and what the dictionary is missing; play-session capture remains the authority on how text actually renders and in which context. The two are complementary.

| Source | Extractable content | Notes |
|---|---|---|
| `%USERPROFILE%\AppData\LocalLow\Jundroo\SimplePlanes 2\DesignerParts.xml` | Part `name`, `category`, `header`, `description` | Read at runtime through `Game.GetPathForDocument`. 188 parts on this machine (0.7.8); `description` is the paragraph shown for a selected part |
| `AircraftThemes.xml` in the same folder | Livery theme `name` | 10 on this machine |
| `Levels.xml` in the same folder | Level `name`, `category`, `description` | 1 on this machine (`Sandbox`) |
| `ControlInputData.xml` in the same folder | Names in the Rewired input configuration | Per the translation rules, input signal names stay English-first with a Chinese note in parentheses |
| `Game.dll`, `Jundroo.Common.dll`, `Jundroo.Packages.dll` under `SimplePlanes 2_Data\Managed` | Hardcoded strings in code (IL `ldstr`) | Dump with `monodis` or `ikdasm` and extract `ldstr`. `Game.dll` holds roughly 8,500 unique strings, part of them UI copy |
| Asset files such as `SimplePlanes 2_Data\resources.assets` | Node text and attribute values inside embedded XML | For example `<Text text="FLY SOLO" />` or `<Style ... text="...">`. Take attribute values and node text; do not take "longest printable runs", or a whole XML blob becomes one string |

Note: the XML files under LocalLow are maintained by the game and may be rewritten on update or reset, so static extraction is an inventory aid only; the plugin does not depend on those files at runtime.
