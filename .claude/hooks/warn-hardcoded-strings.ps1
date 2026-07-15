# PostToolUse hook: flags hardcoded user-facing strings in .cs files.
# Output is shown to Claude as context, prompting it to move strings to the right .resx file.

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

$lines = Get-Content $file_path
$lineNumber = 0
$findings = @()

foreach ($line in $lines) {
    $lineNumber++

    # Skip pure comment lines
    if ($line.TrimStart() -match '^(//|/\*)') { continue }

    # Patterns that indicate a hardcoded user-facing string:
    # SetError("...") / SetSuccess("...") / SetWarning("...")
    # .WithMessage("...") / .Must(...).WithMessage("...")
    # ErrorMessage = "..." (DataAnnotation attribute)
    # Any string literal >= 8 chars with a space that is not a log/format/nameof
    $msgPatterns = @(
        'Set(Error|Success|Warning)\s*\(\s*\$?"[^"]{4,}"',
        '\.WithMessage\s*\(\s*\$?"[^"]{4,}"',
        'ErrorMessage\s*=\s*"[^"]{4,}"',
        '\bMessage\s*=\s*"[^"]{4,}"'
    )

    foreach ($pattern in $msgPatterns) {
        if ($line -match $pattern) {
            $findings += "  Line ${lineNumber}: $($line.Trim())"
            break
        }
    }
}

if ($findings.Count -gt 0) {
    Write-Output ""
    Write-Output "RESX: Hardcoded user-facing string(s) detected in '$file_path'."
    Write-Output "Move each to the matching Features/<Domain>/Resources/*.resx and reference via Resources.<Class>.<Key>."
    $findings | ForEach-Object { Write-Output $_ }
    Write-Output ""
}

exit 0
