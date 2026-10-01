param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[A-Za-z]:\\?$')]
    [string]$Drive
)

$ErrorActionPreference = 'Stop'

$root = [System.IO.Path]::GetPathRoot($Drive)
$markerPath = Join-Path $root '.nohidden-test-fixture.json'

if (-not (Test-Path -LiteralPath $markerPath -PathType Leaf)) {
    throw "No NoHidden test-fixture marker exists on $root. Refusing to remove anything."
}

$marker = Get-Content -LiteralPath $markerPath -Raw | ConvertFrom-Json

if ($marker.Fixture -ne 'NoHidden USB Scanner Test Fixture' -or $marker.Version -ne 1) {
    throw "The fixture marker is invalid. Refusing to remove anything."
}

$paths = @(
    $marker.Paths.HiddenFolder
    $marker.Paths.HiddenFile
    $marker.Paths.DoubleExtension
    $marker.Paths.ImpersonatedFolder
    $marker.Paths.ImpersonatingExecutable
    $marker.Paths.RootScript
    $marker.Paths.HiddenPayload
    $marker.Paths.Autorun
)

foreach ($path in $paths) {
    if ([string]::IsNullOrWhiteSpace($path)) {
        continue
    }

    $fullPath = [System.IO.Path]::GetFullPath([string]$path)
    $rootFullPath = [System.IO.Path]::GetFullPath($root)

    if (-not $fullPath.StartsWith($rootFullPath, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Fixture contains an unsafe path outside the selected drive: $fullPath"
    }

    if (Test-Path -LiteralPath $fullPath -Force) {
        try {
            [System.IO.File]::SetAttributes($fullPath, [System.IO.FileAttributes]::Normal)
        }
        catch {
            # Directories or files may already have been restored/deleted by NoHidden.
        }

        Remove-Item -LiteralPath $fullPath -Recurse -Force
    }
}

Remove-Item -LiteralPath $markerPath -Force

Write-Host "NoHidden test fixture removed from $root." -ForegroundColor Green
