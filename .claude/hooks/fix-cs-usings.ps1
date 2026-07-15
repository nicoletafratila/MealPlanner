# PostToolUse hook: ensures each `using` directive in a .cs file is on its own line.
# Receives tool call info as JSON on stdin.

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

$content = [System.IO.File]::ReadAllText($file_path)

# Detect line ending style
$newline = if ($content.Contains("`r`n")) { "`r`n" } else { "`n" }

# Split concatenated usings: "using A;using B;" or "using A; using B;"
# The lookbehind (?<=;) matches position right after a semicolon,
# then eats any horizontal whitespace, then lookahead ensures "using " follows.
$fixed = $content -replace '(?<=;)[ \t]*(?=using )', $newline

if ($fixed -ne $content) {
    [System.IO.File]::WriteAllText($file_path, $fixed, [System.Text.UTF8Encoding]::new($false))
}
