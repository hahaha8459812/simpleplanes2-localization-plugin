param(
    [string]$GameDir = "E:\Game\steam\steamapps\common\SimplePlanes 2",
    [switch]$InstallToGame
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
chcp 65001 > $null

$ErrorActionPreference = "Stop"

$projectRoot = $PSScriptRoot
$workspaceRoot = Split-Path -Parent $projectRoot
$artifactsDir = Join-Path $projectRoot "artifacts"
$releaseRoot = Join-Path $projectRoot "release"
$pluginOutput = Join-Path $artifactsDir "SimplePlanes2Translation.dll"
$fontArtifactsDir = Join-Path $artifactsDir "fonts"
$translationSourceRoot = Join-Path $projectRoot "content\translations\zh-CN.fragments"
$translationOutputPath = Join-Path $projectRoot "content\translations\zh-CN.json"
$bepInExVersion = "5.4.23.5"
$bepInExZipPath = Join-Path $workspaceRoot ".deps\bepinex\BepInEx_win_x64_${bepInExVersion}.zip"
$bepInExExtractPath = Join-Path $workspaceRoot ".deps\bepinex\$bepInExVersion"
$fontDepsRoot = Join-Path $workspaceRoot ".deps\fonts"
$sourceHanReleaseTag = "2.005R"
$sourceHanZipPath = Join-Path $fontDepsRoot "09_SourceHanSansSC.zip"
$sourceHanExtractPath = Join-Path $fontDepsRoot "sourcehansanssc"
$bundledFontFileName = "SourceHanSansSC-Regular.otf"
$bundledFontLicenseFileName = "SourceHanSansSC-LICENSE.txt"
$sourceHanDownloadUrl = "https://github.com/adobe-fonts/source-han-sans/releases/download/${sourceHanReleaseTag}/09_SourceHanSansSC.zip"
$managedDir = Join-Path $GameDir "SimplePlanes 2_Data\Managed"

function Get-CSharpCompilerPath {
    $candidates = @(
        "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
        "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe",
        "C:\Program Files\Unity\Hub\Editor\2022.3.22f1\Editor\Data\MonoBleedingEdge\lib\mono\4.5\csc.exe",
        "C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Data\MonoBleedingEdge\lib\mono\4.5\csc.exe"
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    throw "Unable to find a usable csc.exe."
}

function Ensure-BepInExPayload {
    $corePath = Join-Path $bepInExExtractPath "BepInEx\core\BepInEx.dll"
    if (Test-Path $corePath) {
        return
    }

    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $bepInExZipPath) | Out-Null
    Invoke-WebRequest "https://github.com/BepInEx/BepInEx/releases/download/v${bepInExVersion}/BepInEx_win_x64_${bepInExVersion}.zip" -OutFile $bepInExZipPath

    if (Test-Path $bepInExExtractPath) {
        Remove-Item -Recurse -Force $bepInExExtractPath
    }

    New-Item -ItemType Directory -Force -Path $bepInExExtractPath | Out-Null
    tar -xf $bepInExZipPath -C $bepInExExtractPath
}

function Copy-DirectoryContents {
    param(
        [string]$Source,
        [string]$Destination
    )

    New-Item -ItemType Directory -Force -Path $Destination | Out-Null
    Copy-Item -Path (Join-Path $Source "*") -Destination $Destination -Recurse -Force
}

function Get-SystemDrawingReferencePath {
    $candidates = @(
        "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Drawing.dll",
        "C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Drawing.dll"
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    throw "Unable to find System.Drawing.dll."
}

function Ensure-BundledChineseFontPayload {
    param(
        [string]$OutputDirectory
    )

    $fontSourcePath = Join-Path $sourceHanExtractPath "OTF\SimplifiedChinese\$bundledFontFileName"
    $licenseSourcePath = Join-Path $sourceHanExtractPath "LICENSE.txt"

    if (!(Test-Path $fontSourcePath) -or !(Test-Path $licenseSourcePath)) {
        New-Item -ItemType Directory -Force -Path $fontDepsRoot | Out-Null

        if (!(Test-Path $sourceHanZipPath)) {
            Invoke-WebRequest $sourceHanDownloadUrl -OutFile $sourceHanZipPath -Headers @{ "User-Agent" = "Codex" }
        }

        if (Test-Path $sourceHanExtractPath) {
            Remove-Item -LiteralPath $sourceHanExtractPath -Recurse -Force
        }

        Expand-Archive -LiteralPath $sourceHanZipPath -DestinationPath $sourceHanExtractPath -Force
    }

    if (!(Test-Path $fontSourcePath)) {
        throw "Bundled Chinese font file was not found after extraction: ${fontSourcePath}"
    }

    if (!(Test-Path $licenseSourcePath)) {
        throw "Bundled Chinese font license file was not found after extraction: ${licenseSourcePath}"
    }

    New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
    Copy-Item -LiteralPath $fontSourcePath -Destination (Join-Path $OutputDirectory $bundledFontFileName) -Force
    Copy-Item -LiteralPath $licenseSourcePath -Destination (Join-Path $OutputDirectory $bundledFontLicenseFileName) -Force
}

function New-Package {
    param(
        [string]$PackageName,
        [string]$SettingsTemplatePath
    )

    $packageRoot = Join-Path $releaseRoot $PackageName
    $packageFilesDir = Join-Path $packageRoot "files"
    $packagePluginDir = Join-Path $packageFilesDir "BepInEx\plugins\SimplePlanes2Translation"
    $packageFontDir = Join-Path $packagePluginDir "fonts"
    $packageTranslationDir = Join-Path $packagePluginDir "translations"
    $zipPath = Join-Path $releaseRoot ($PackageName + ".zip")

    if (Test-Path $packageRoot) {
        Remove-Item -Recurse -Force $packageRoot
    }

    if (Test-Path $zipPath) {
        Remove-Item -Force $zipPath
    }

    New-Item -ItemType Directory -Force -Path $packageTranslationDir | Out-Null
    New-Item -ItemType Directory -Force -Path $packageFontDir | Out-Null

    Copy-DirectoryContents -Source $bepInExExtractPath -Destination $packageFilesDir
    Copy-Item -Path $pluginOutput -Destination (Join-Path $packagePluginDir "SimplePlanes2Translation.dll") -Force
    Copy-Item -Path $SettingsTemplatePath -Destination (Join-Path $packagePluginDir "settings.json") -Force
    Copy-Item -Path (Join-Path $projectRoot "content\translations\*.json") -Destination $packageTranslationDir -Force
    Copy-Item -Path (Join-Path $fontArtifactsDir "*") -Destination $packageFontDir -Force
    Copy-Item -Path (Join-Path $projectRoot "install.ps1") -Destination (Join-Path $packageRoot "install.ps1") -Force
    Copy-Item -Path (Join-Path $projectRoot "README.md") -Destination (Join-Path $packageRoot "README.md") -Force
    Copy-Item -Path (Join-Path $projectRoot "README.en.md") -Destination (Join-Path $packageRoot "README.en.md") -Force
    Copy-Item -Path (Join-Path $projectRoot "CHANGELOG.md") -Destination (Join-Path $packageRoot "CHANGELOG.md") -Force
    Copy-DirectoryContents -Source (Join-Path $projectRoot "docs") -Destination (Join-Path $packageRoot "docs")

    Compress-Archive -Path $packageRoot -DestinationPath $zipPath -CompressionLevel Optimal

    return $packageRoot
}

function Merge-TranslationFragments {
    param(
        [string]$SourceRoot,
        [string]$OutputPath
    )

    $fragmentFiles = @()
    $entryNames = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
    $entryRows = New-Object 'System.Collections.Generic.List[object]'
    $contextRuleKeys = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
    $contextRows = New-Object 'System.Collections.Generic.List[object]'
    $builder = New-Object System.Text.StringBuilder
    $isFirstEntry = $true
    $isFirstContextEntry = $true

    if (!(Test-Path $SourceRoot)) {
        return
    }

    $fragmentFiles = Get-ChildItem -Path $SourceRoot -Filter *.json | Sort-Object Name
    foreach ($fragmentFile in $fragmentFiles) {
        $fragment = Get-Content $fragmentFile.FullName -Raw -Encoding UTF8 | ConvertFrom-Json
        $entries = @()
        if ($fragment.entries) {
            $entries = @($fragment.entries)
        }

        foreach ($entry in $entries) {
            $key = [string]$entry.key
            $value = [string]$entry.value

            if ([string]::IsNullOrEmpty($key)) {
                continue
            }

            if (!$entryNames.Add($key)) {
                throw "Duplicate translation key '${key}' found in fragment '${($fragmentFile.Name)}'."
            }

            $entryRows.Add([PSCustomObject]@{
                key = $key
                value = $value
            }) | Out-Null
        }

        $contextEntries = @()
        if ($fragment.contextEntries) {
            $contextEntries = @($fragment.contextEntries)
        }

        foreach ($contextEntry in $contextEntries) {
            $key = [string]$contextEntry.key
            $value = [string]$contextEntry.value
            $sceneName = [string]$contextEntry.sceneName
            $gameObjectPathContains = [string]$contextEntry.gameObjectPathContains
            $parentPathContains = [string]$contextEntry.parentPathContains
            $objectName = [string]$contextEntry.objectName
            $parentName = [string]$contextEntry.parentName
            $componentType = [string]$contextEntry.componentType
            $siblingIndex = $contextEntry.siblingIndex
            $anchoredPosition = [string]$contextEntry.anchoredPosition
            $ruleKey = @(
                $key,
                $sceneName,
                $gameObjectPathContains,
                $parentPathContains,
                $objectName,
                $parentName,
                $componentType,
                [string]$siblingIndex,
                $anchoredPosition
            ) -join "`u001f"

            if ([string]::IsNullOrEmpty($key)) {
                continue
            }

            if ($value -eq $null) {
                continue
            }

            if (!$contextRuleKeys.Add($ruleKey)) {
                throw "Duplicate context translation rule for key '${key}' found in fragment '${($fragmentFile.Name)}'."
            }

            $contextRows.Add([PSCustomObject]@{
                key = $key
                value = $value
                sceneName = $sceneName
                gameObjectPathContains = $gameObjectPathContains
                parentPathContains = $parentPathContains
                objectName = $objectName
                parentName = $parentName
                componentType = $componentType
                siblingIndex = $siblingIndex
                anchoredPosition = $anchoredPosition
            }) | Out-Null
        }
    }

    [void]$builder.AppendLine("{")
    [void]$builder.AppendLine('  "entries": {')

    foreach ($row in $entryRows) {
        $escapedKey = [System.Web.HttpUtility]::JavaScriptStringEncode($row.key)
        $escapedValue = [System.Web.HttpUtility]::JavaScriptStringEncode($row.value)

        if (!$isFirstEntry) {
            [void]$builder.AppendLine(",")
        }

        [void]$builder.Append('    "')
        [void]$builder.Append($escapedKey)
        [void]$builder.Append('": "')
        [void]$builder.Append($escapedValue)
        [void]$builder.Append('"')
        $isFirstEntry = $false
    }

    [void]$builder.AppendLine()
    [void]$builder.AppendLine("  },")
    [void]$builder.AppendLine('  "contextEntries": [')

    foreach ($row in $contextRows) {
        if (!$isFirstContextEntry) {
            [void]$builder.AppendLine(",")
        }

        [void]$builder.Append('    {')
        [void]$builder.Append('"key": "')
        [void]$builder.Append([System.Web.HttpUtility]::JavaScriptStringEncode([string]$row.key))
        [void]$builder.Append('", "value": "')
        [void]$builder.Append([System.Web.HttpUtility]::JavaScriptStringEncode([string]$row.value))
        [void]$builder.Append('"')

        if (![string]::IsNullOrEmpty([string]$row.sceneName)) {
            [void]$builder.Append(', "sceneName": "')
            [void]$builder.Append([System.Web.HttpUtility]::JavaScriptStringEncode([string]$row.sceneName))
            [void]$builder.Append('"')
        }

        if (![string]::IsNullOrEmpty([string]$row.gameObjectPathContains)) {
            [void]$builder.Append(', "gameObjectPathContains": "')
            [void]$builder.Append([System.Web.HttpUtility]::JavaScriptStringEncode([string]$row.gameObjectPathContains))
            [void]$builder.Append('"')
        }

        if (![string]::IsNullOrEmpty([string]$row.parentPathContains)) {
            [void]$builder.Append(', "parentPathContains": "')
            [void]$builder.Append([System.Web.HttpUtility]::JavaScriptStringEncode([string]$row.parentPathContains))
            [void]$builder.Append('"')
        }

        if (![string]::IsNullOrEmpty([string]$row.objectName)) {
            [void]$builder.Append(', "objectName": "')
            [void]$builder.Append([System.Web.HttpUtility]::JavaScriptStringEncode([string]$row.objectName))
            [void]$builder.Append('"')
        }

        if (![string]::IsNullOrEmpty([string]$row.parentName)) {
            [void]$builder.Append(', "parentName": "')
            [void]$builder.Append([System.Web.HttpUtility]::JavaScriptStringEncode([string]$row.parentName))
            [void]$builder.Append('"')
        }

        if (![string]::IsNullOrEmpty([string]$row.componentType)) {
            [void]$builder.Append(', "componentType": "')
            [void]$builder.Append([System.Web.HttpUtility]::JavaScriptStringEncode([string]$row.componentType))
            [void]$builder.Append('"')
        }

        if ($row.siblingIndex -ne $null -and [string]$row.siblingIndex -ne "") {
            [void]$builder.Append(', "siblingIndex": ')
            [void]$builder.Append([int]$row.siblingIndex)
        }

        if (![string]::IsNullOrEmpty([string]$row.anchoredPosition)) {
            [void]$builder.Append(', "anchoredPosition": "')
            [void]$builder.Append([System.Web.HttpUtility]::JavaScriptStringEncode([string]$row.anchoredPosition))
            [void]$builder.Append('"')
        }

        [void]$builder.Append('}')
        $isFirstContextEntry = $false
    }

    [void]$builder.AppendLine()
    [void]$builder.AppendLine("  ]")
    [void]$builder.AppendLine("}")

    [System.IO.File]::WriteAllText($OutputPath, $builder.ToString(), [System.Text.Encoding]::UTF8)
}

if (!(Test-Path (Join-Path $GameDir "SimplePlanes 2.exe"))) {
    throw "Invalid game directory: ${GameDir}"
}

if (!(Test-Path $managedDir)) {
    throw "Managed assemblies directory was not found: ${managedDir}"
}

Ensure-BepInExPayload

$compilerPath = Get-CSharpCompilerPath

if (Test-Path $artifactsDir) {
    Remove-Item -Recurse -Force $artifactsDir
}

if (Test-Path $releaseRoot) {
    Remove-Item -Recurse -Force $releaseRoot
}

New-Item -ItemType Directory -Force -Path $artifactsDir | Out-Null
New-Item -ItemType Directory -Force -Path $releaseRoot | Out-Null
Ensure-BundledChineseFontPayload -OutputDirectory $fontArtifactsDir

Merge-TranslationFragments -SourceRoot $translationSourceRoot -OutputPath $translationOutputPath

$references = @(
    (Join-Path $bepInExExtractPath "BepInEx\core\BepInEx.dll"),
    (Join-Path $bepInExExtractPath "BepInEx\core\0Harmony.dll"),
    (Join-Path $managedDir "Jundroo.Packages.dll"),
    (Join-Path $managedDir "Unity.TextMeshPro.dll"),
    (Join-Path $managedDir "Newtonsoft.Json.dll"),
    (Join-Path $managedDir "UnityEngine.dll"),
    (Join-Path $managedDir "UnityEngine.CoreModule.dll"),
    (Join-Path $managedDir "UnityEngine.InputLegacyModule.dll"),
    (Join-Path $managedDir "UnityEngine.UI.dll"),
    (Join-Path $managedDir "UnityEngine.TextCoreTextEngineModule.dll"),
    (Join-Path $managedDir "UnityEngine.TextRenderingModule.dll"),
    (Join-Path $managedDir "netstandard.dll")
)

$references += Get-SystemDrawingReferencePath

$sourceFiles = Get-ChildItem -Path (Join-Path $projectRoot "src") -Filter *.cs | Sort-Object Name | Select-Object -ExpandProperty FullName
$compilerArgs = @(
    "/nologo",
    "/target:library",
    "/langversion:5",
    "/optimize+",
    "/out:$pluginOutput"
)

foreach ($reference in $references) {
    $compilerArgs += "/reference:$reference"
}

$compilerArgs += $sourceFiles

& $compilerPath @compilerArgs
if ($LASTEXITCODE -ne 0) {
    throw "Compilation failed."
}

$devPackageRoot = New-Package -PackageName "SimplePlanes2TranslationMod-Dev" -SettingsTemplatePath (Join-Path $projectRoot "content\settings.dev.json")
$releasePackageRoot = New-Package -PackageName "SimplePlanes2TranslationMod-Release" -SettingsTemplatePath (Join-Path $projectRoot "content\settings.release.json")

if ($InstallToGame) {
    Copy-DirectoryContents -Source (Join-Path $releasePackageRoot "files") -Destination $GameDir
}

Write-Host "Build completed: ${releaseRoot}"
