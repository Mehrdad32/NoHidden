param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[A-Za-z]:\\?$')]
    [string]$Drive
)

$ErrorActionPreference = 'Stop'

$root = [System.IO.Path]::GetPathRoot($Drive)

if ([string]::IsNullOrWhiteSpace($root)) {
    throw "Invalid drive path."
}

$driveInfo = [System.IO.DriveInfo]::new($root)

if (-not $driveInfo.IsReady) {
    throw "Drive $root is not ready."
}

if ($driveInfo.DriveType -ne [System.IO.DriveType]::Removable) {
    throw "Refusing to create the fixture on $root because Windows does not report it as a removable drive."
}

$markerPath = Join-Path $root '.nohidden-test-fixture.json'

$items = [ordered]@{
    HiddenFolder = Join-Path $root 'NH-Test-HiddenFolder'
    HiddenFile = Join-Path $root 'NH-Test-HiddenNote.txt'
    DoubleExtension = Join-Path $root 'NH-Test-Photo.jpg.exe'
    ImpersonatedFolder = Join-Path $root 'NH-Test-Documents'
    ImpersonatingExecutable = Join-Path $root 'NH-Test-Documents.exe'
    RootScript = Join-Path $root 'NH-Test-Review.bat'
    HiddenPayload = Join-Path $root 'NH-Test-Payload.vbs'
    Autorun = Join-Path $root 'autorun.inf'
}

$conflicts = @($markerPath) + @($items.Values) | Where-Object { Test-Path -LiteralPath $_ }

if ($conflicts.Count -gt 0) {
    $formatted = $conflicts -join [Environment]::NewLine
    throw "Test fixture was not created because these paths already exist:$([Environment]::NewLine)$formatted"
}

Write-Host "Creating harmless NoHidden test fixture on $root ..." -ForegroundColor Cyan

New-Item -ItemType Directory -Path $items.HiddenFolder | Out-Null
Set-Content -LiteralPath (Join-Path $items.HiddenFolder 'RecoveredFile.txt') -Value 'NoHidden harmless recovery test.'
[System.IO.File]::SetAttributes(
    $items.HiddenFolder,
    ([System.IO.File]::GetAttributes($items.HiddenFolder) -bor [System.IO.FileAttributes]::Hidden -bor [System.IO.FileAttributes]::System))

Set-Content -LiteralPath $items.HiddenFile -Value 'NoHidden harmless hidden-file recovery test.'
[System.IO.File]::SetAttributes(
    $items.HiddenFile,
    ([System.IO.File]::GetAttributes($items.HiddenFile) -bor [System.IO.FileAttributes]::Hidden -bor [System.IO.FileAttributes]::System))

Set-Content -LiteralPath $items.DoubleExtension -Value 'This is plain text, not an executable.'
New-Item -ItemType Directory -Path $items.ImpersonatedFolder | Out-Null
Set-Content -LiteralPath (Join-Path $items.ImpersonatedFolder 'Readme.txt') -Value 'NoHidden folder impersonation test.'
Set-Content -LiteralPath $items.ImpersonatingExecutable -Value 'This is plain text, not an executable.'

Set-Content -LiteralPath $items.RootScript -Value @(
    '@echo off'
    'rem Harmless NoHidden scanner fixture. Do not execute user files automatically.'
)

Set-Content -LiteralPath $items.HiddenPayload -Value @(
    "' Harmless NoHidden scanner fixture"
    "' This script intentionally performs no action."
)
[System.IO.File]::SetAttributes(
    $items.HiddenPayload,
    ([System.IO.File]::GetAttributes($items.HiddenPayload) -bor [System.IO.FileAttributes]::Hidden -bor [System.IO.FileAttributes]::System))

Set-Content -LiteralPath $items.Autorun -Value @(
    '[autorun]'
    'open=wscript.exe NH-Test-Payload.vbs'
)

$marker = [ordered]@{
    Fixture = 'NoHidden USB Scanner Test Fixture'
    Version = 1
    CreatedUtc = [DateTime]::UtcNow.ToString('O')
    DriveRoot = $root
    Harmless = $true
    Paths = $items
}

$marker | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $markerPath -Encoding UTF8

Write-Host ''
Write-Host 'Fixture created successfully.' -ForegroundColor Green
Write-Host 'Expected NoHidden findings:' -ForegroundColor Yellow
Write-Host '  - NH-Test-HiddenFolder: recoverable Hidden/System item'
Write-Host '  - NH-Test-HiddenNote.txt: recoverable Hidden/System item'
Write-Host '  - NH-Test-Photo.jpg.exe: High risk double extension'
Write-Host '  - NH-Test-Documents.exe: High risk folder impersonation'
Write-Host '  - NH-Test-Review.bat: suspicious root script'
Write-Host '  - NH-Test-Payload.vbs: High risk hidden script; MUST NOT offer Restore visibility'
Write-Host '  - autorun.inf: High risk suspicious autorun'
Write-Host ''
Write-Host 'After testing, run scripts\Remove-NoHiddenTestUsb.ps1 for the same drive.' -ForegroundColor Cyan
