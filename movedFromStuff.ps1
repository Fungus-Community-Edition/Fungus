# Run from repo root
$root = "Assets\Amanita\Systems\VScripting"

Get-ChildItem -Recurse $root -Filter *.cs | ForEach-Object {
    $path = $_.FullName
    $text = Get-Content $path -Raw

    # 1) Ensure using UnityEngine.Scripting.APIUpdating;
    if ($text -notmatch 'using\s+UnityEngine\.Scripting\.APIUpdating\s*;') {
        # Insert after last using or before first namespace
        $text = $text -replace '(^using\b[\s\S]*?;\s*)\r?\n(\r?\n)*', "`$1`r`nusing UnityEngine.Scripting.APIUpdating;`r`n`r`n"
    }

    # 2) Ensure [MovedFrom] above class declaration
    if ($text -notmatch '\[MovedFrom\("AtMycelia\.Amanita\.VScripting"\)\]') {
        $text = $text -replace '(\r?\n\s*)(public|internal|protected|private)?\s*(abstract\s+|sealed\s+|static\s+)?class\s+',
            "`r`n[MovedFrom(`"AtMycelia.Amanita.VScripting`")]`r`n`$1`$2 `$3class "
    }

    Set-Content $path $text -NoNewline
}