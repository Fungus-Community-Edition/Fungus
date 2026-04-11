$root = "Assets\Amanita\Systems\VScripting"

Get-ChildItem -Recurse $root -Filter *.cs | ForEach-Object {
    $path = $_.FullName
    $text = Get-Content $path -Raw

    if ([string]::IsNullOrWhiteSpace($text)) {
        Write-Host "Skipped (empty): $path"
        return
    }

    $needsUsing = $text -notmatch 'using\s+UnityEngine\.Scripting\.APIUpdating\s*;'
    $needsMovedFrom = $text -notmatch '\[MovedFrom\("AtMycelia\.Amanita\.VScripting"\)\]'

    if ($needsUsing -or $needsMovedFrom) {
        Write-Host "Would update: $path"
        if ($needsUsing) { Write-Host "  - add using UnityEngine.Scripting.APIUpdating;" }
        if ($needsMovedFrom) { Write-Host "  - add [MovedFrom(\"AtMycelia.Amanita.VScripting\")]" }
    }
}