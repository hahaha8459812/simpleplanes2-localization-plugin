param(
    [string]$GameDir = "E:\Game\steam\steamapps\common\SimplePlanes 2"
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
chcp 65001 > $null

$ErrorActionPreference = "Stop"

$nestedPayloadRoot = Join-Path $PSScriptRoot "files"
$payloadRoot = $PSScriptRoot
$gameExePath = Join-Path $GameDir "SimplePlanes 2.exe"
$payloadNames = @(
    "BepInEx",
    ".doorstop_version",
    "changelog.txt",
    "doorstop_config.ini",
    "winhttp.dll"
)

if (Test-Path $nestedPayloadRoot) {
    $payloadRoot = $nestedPayloadRoot
}

if (!(Test-Path $gameExePath)) {
    throw "Target directory is not a SimplePlanes 2 install: ${GameDir}"
}

$installedPayloadCount = 0
foreach ($payloadName in $payloadNames) {
    $payloadPath = Join-Path $payloadRoot $payloadName
    if (!(Test-Path $payloadPath)) {
        continue
    }

    Copy-Item -LiteralPath $payloadPath -Destination $GameDir -Recurse -Force
    $installedPayloadCount += 1
}

if ($installedPayloadCount -eq 0) {
    throw "No install payload was found in: ${payloadRoot}"
}

Write-Host ""
Write-Host "Install completed."
Write-Host "Game directory: ${GameDir}"
Write-Host "Plugin directory: $(Join-Path $GameDir 'BepInEx\plugins\SimplePlanes2Translation')"
Write-Host "If English text remains after first launch, press F6 to reload translations."
