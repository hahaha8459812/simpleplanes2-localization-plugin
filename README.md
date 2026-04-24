# SimplePlanes 2 Localization Plugin

`simpleplanes2-localization-plugin` is a runtime localization plugin for `SimplePlanes 2`.

The current implementation focuses on Simplified Chinese localization. It uses `BepInEx + Harmony` to translate Unity UI text at runtime, supports text collection for translation work, and bundles a Chinese font so translated UI can render reliably in game.

Main project files are in:

```text
translation-mod/
```

Start here:

- [Project README](translation-mod/README.md)
- [Translation workflow](translation-mod/docs/TRANSLATION_WORKFLOW.md)
- [Release checklist](translation-mod/docs/RELEASE_CHECKLIST.md)
- [Terminology](translation-mod/content/translations/zh-CN.fragments/TERMS.md)

Build from `translation-mod`:

```powershell
.\build.ps1
```

Generated packages are written to `translation-mod/release/`.
