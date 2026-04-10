param([switch]$WhatIf)

$root = "Assets\Amanita\Systems\VScripting"
$usingLine = "using UnityEngine.Scripting.APIUpdating;"
$movedAttr = "[MovedFrom(`"AtMycelia.Amanita.VScripting`")]"
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

Get-ChildItem -Recurse $root -Filter *.cs | ForEach-Object {
    $path = $_.FullName
    $text = [System.IO.File]::ReadAllText($path)

    if ([string]::IsNullOrWhiteSpace($text)) {
        Write-Host "Skipped (empty): $path"
        return
    }

    $updated = $text

    if ($updated -notmatch 'using\s+UnityEngine\.Scripting\.APIUpdating\s*;') {
        $ns = "namespace AtMycelia.Hyphlow"
        $idx = $updated.IndexOf($ns)
        if ($idx -ge 0) {
            $updated = $updated.Substring(0, $idx) + $usingLine + "`r`n`r`n" + $updated.Substring($idx)
        }
    }

    if ($updated -notmatch '\[MovedFrom\("AtMycelia\.Amanita\.VScripting"\)\]') {
        $lines = $updated -split "`r`n"
        for ($i = 0; $i -lt $lines.Length; $i++) {
            if ($lines[$i] -match '^\s*(public|internal|protected|private)?\s*(abstract\s+|sealed\s+|static\s+)?class\s+') {
                $lines = $lines[0..($i-1)] + $movedAttr + $lines[$i..($lines.Length-1)]
                break
            }
        }
        $updated = ($lines -join "`r`n")
    }

    if ($updated -ne $text) {
        Write-Host "Would update: $path"
        if (-not $WhatIf) {
            Copy-Item $path "$path.bak" -Force
            $tmp = "$path.tmp"
            [System.IO.File]::WriteAllText($tmp, $updated, $utf8NoBom)
            Move-Item $tmp $path -Force
            Write-Host "Updated: $path"
        }
    }
}