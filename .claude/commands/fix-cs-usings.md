---
name: fix-cs-usings
description: Split concatenated C# using directives so each one is on its own line
---

Fix C# `using` directives so each one is on its own line. Finds lines where multiple `using` statements are concatenated (e.g. `using A;using B;` or `using A; using B;`) and splits them so every `using` appears on a separate line.

## Steps

1. If a specific file was mentioned, use it. Otherwise ask which file(s) to fix.
2. Read the file with the Read tool.
3. Identify all lines containing two or more concatenated `using` statements.
4. Use the Edit tool to replace each such line with the `using` directives split across individual lines, preserving the file's existing indentation and line-ending style.
5. Report how many lines were fixed.
