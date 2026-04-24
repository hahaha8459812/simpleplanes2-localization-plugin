param(
    [string]$GameDir = "E:\Game\steam\steamapps\common\SimplePlanes 2"
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
chcp 65001 > $null

$ErrorActionPreference = "Stop"

$payloadRoot = Join-Path $PSScriptRoot "files"
$gameExePath = Join-Path $GameDir "SimplePlanes 2.exe"

if (!(Test-Path $payloadRoot)) {
    throw "Install payload directory was not found: ${payloadRoot}"
}

if (!(Test-Path $gameExePath)) {
    throw "Target directory is not a SimplePlanes 2 install: ${GameDir}"
}

Copy-Item -Path (Join-Path $payloadRoot "*") -Destination $GameDir -Recurse -Force

Write-Host ""
Write-Host "Install completed."
Write-Host "Game directory: ${GameDir}"
Write-Host "Plugin directory: $(Join-Path $GameDir 'BepInEx\plugins\SimplePlanes2Translation')"
Write-Host "If English text remains after first launch, press F6 to reload translations."
