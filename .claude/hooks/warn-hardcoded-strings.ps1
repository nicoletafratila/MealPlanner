# PostToolUse hook: flags hardcoded user-facing strings in .cs AND .xaml files.
# Output is shown to Claude as context, prompting it to move strings to the right .resx file.

$raw = [System.Console]::In.ReadToEnd()

try {
    $data = $raw | ConvertFrom-Json
    $file_path = $data.tool_input.file_path
} catch {
    exit 0
}

if (-not $file_path) {
    exit 0
}

if (-not (Test-Path $file_path)) {
    exit 0
}

$isCs = $file_path -match '\.cs$'
$isXaml = $file_path -match '\.xaml$'

if (-not $isCs -and -not $isXaml) {
    exit 0
}

# Never flag generated resource classes.
if ($file_path -match '\.Designer\.cs$') {
    exit 0
}

$lines = Get-Content $file_path
$lineNumber = 0
$findings = @()

# .cs patterns that indicate a hardcoded user-facing string:
# SetError("...") / SetSuccess("...") / SetWarning("...")
# .WithMessage("...") / .Must(...).WithMessage("...")
# ErrorMessage = "..." (DataAnnotation attribute)
# Message = "..."
$csPatterns = @(
    'Set(Error|Success|Warning)\s*\(\s*\$?"[^"]{4,}"',
    '\.WithMessage\s*\(\s*\$?"[^"]{4,}"',
    'ErrorMessage\s*=\s*"[^"]{4,}"',
    '\bMessage\s*=\s*"[^"]{4,}"'
)

# .xaml attributes that render user-facing text. A hardcoded literal is one whose
# value is NOT a binding/markup extension (does not start with '{') and contains a letter.
$xamlAttrs = 'Text|Title|Placeholder|ToolTipProperties\.Text|SemanticProperties\.Description|SemanticProperties\.Hint'

foreach ($line in $lines) {
    $lineNumber++

    if ($isCs) {
        # Skip pure comment lines
        if ($line.TrimStart() -match '^(//|/\*)') { continue }

        foreach ($pattern in $csPatterns) {
            if ($line -match $pattern) {
                $findings += "  Line ${lineNumber}: $($line.Trim())"
                break
            }
        }
    }
    elseif ($isXaml) {
        # Skip XML comment lines
        if ($line.TrimStart() -match '^<!--') { continue }

        # Match e.g. Text="Some words"  but NOT Text="{Binding ...}" / Text="{x:Static ...}"
        # Require the literal to contain at least one letter (skips emoji/symbol-only values).
        if ($line -match "(?:$xamlAttrs)\s*=\s*`"(?!\s*\{)([^`"]*[A-Za-z][^`"]*)`"") {
            $findings += "  Line ${lineNumber}: $($line.Trim())"
        }
    }
}

if ($findings.Count -gt 0) {
    Write-Output ""
    Write-Output "RESX: Hardcoded user-facing string(s) detected in '$file_path'."
    if ($isCs) {
        Write-Output "Move each to the matching Features/<Domain>/Resources/*.resx (or the closest *.Shared/Resources) and reference via Resources.<Class>.<Key>."
    }
    else {
        Write-Output "Move each to MealPlanner.UI.Mobile\Resources\Strings\AppStrings.resx and reference in XAML via {x:Static strings:AppStrings.<Key>}."
    }
    Write-Output "See the /use-resx-strings skill for the full procedure."
    $findings | ForEach-Object { Write-Output $_ }
    Write-Output ""
}

exit 0
