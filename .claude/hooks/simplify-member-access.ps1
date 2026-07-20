# PostToolUse hook: applies the "Simplify member access" (IDE0001/IDE0002) code style
# fix to the edited .cs file. Runs as an async hook since dotnet format's per-invocation
# analyzer/design-time-build overhead is too slow to run synchronously on every edit.
# Receives tool call info as JSON on stdin.
#
# NOTE: dotnet format's --include only matches files given as paths relative to the
# repo root (cwd) - absolute paths are silently skipped (0 files formatted). So every
# path handed to dotnet format below is first made relative to the repo root.

$raw = [System.Console]::In.ReadToEnd()

try {
    $data = $raw | ConvertFrom-Json
    $file_path = $data.tool_input.file_path
} catch {
    exit 0
}

if (-not $file_path -or $file_path -notmatch '\.cs$') {
    exit 0
}

if (-not (Test-Path $file_path)) {
    exit 0
}

if ($file_path -match '\.Designer\.cs$') {
    exit 0
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path.TrimEnd('\')

function Get-RelativeToRepoRoot($absolutePath) {
    $normalized = $absolutePath -replace '/', '\'
    if ($normalized.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $normalized.Substring($repoRoot.Length).TrimStart('\')
    }
    return $normalized
}

# Walk up from the file to find its nearest .csproj, so dotnet format only has to
# load that project's compilation instead of the whole solution.
$dir = Split-Path -Parent $file_path
$csproj = $null
while ($dir -and -not $csproj) {
    $candidates = Get-ChildItem -Path $dir -Filter *.csproj -File -ErrorAction SilentlyContinue
    if ($candidates) {
        $csproj = $candidates[0].FullName
    } else {
        $parent = Split-Path -Parent $dir
        if ($parent -eq $dir) { break }
        $dir = $parent
    }
}

if (-not $csproj) {
    exit 0
}

$relCsproj = Get-RelativeToRepoRoot $csproj
$relFile = Get-RelativeToRepoRoot $file_path

Push-Location $repoRoot
try {
    dotnet format style $relCsproj --include $relFile --diagnostics IDE0001 IDE0002 --severity info --verbosity quiet 2>$null | Out-Null
} finally {
    Pop-Location
}

exit 0
