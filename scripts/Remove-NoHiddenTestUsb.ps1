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

$rootFullPath = [System.IO.Path]::GetFullPath($root)

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
    if ([string]::IsNullOrWhiteSpace([string]$path)) {
        continue
    }

    $fullPath = [System.IO.Path]::GetFullPath([string]$path)

    if (-not $fullPath.StartsWith($rootFullPath, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Fixture contains an unsafe path outside the selected drive: $fullPath"
    }

    $candidates = [System.Collections.Generic.List[string]]::new()
    $candidates.Add($fullPath)

    $parentDirectory = Split-Path -Parent $fullPath
    $leafName = Split-Path -Leaf $fullPath

    if (Test-Path -LiteralPath $parentDirectory -PathType Container) {
        $neutralizedPrefix = $leafName + '.nohidden-disabled'

        Get-ChildItem -LiteralPath $parentDirectory -Force -ErrorAction SilentlyContinue |
            Where-Object {
                $_.Name -eq $neutralizedPrefix -or
                $_.Name -match ('^' + [regex]::Escape($neutralizedPrefix) + '\\.\\d+$')
            } |
            ForEach-Object {
                $candidates.Add($_.FullName)
            }
    }

    foreach ($candidate in ($candidates | Select-Object -Unique)) {
        if (-not (Test-Path -LiteralPath $candidate)) {
            continue
        }

        try {
            [System.IO.File]::SetAttributes(
                $candidate,
                [System.IO.FileAttributes]::Normal)
        }
        catch {
            # The item may be a directory or may already have been changed by NoHidden.
        }

        Remove-Item -LiteralPath $candidate -Recurse -Force
    }
}

Remove-Item -LiteralPath $markerPath -Force

Write-Host "NoHidden test fixture removed from $root." -ForegroundColor Green
