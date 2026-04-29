from __future__ import annotations

from pathlib import Path
import re
import sys


BINDING_MAP = {
    "key": "_key",
    "value": "_value",
    "scope": "_scope",
}

BINDING_ATTR_PATTERN = re.compile(r'(binding-path|bindingPath)\s*=\s*"([^"]*)"')


def update_bindings(text: str) -> tuple[str, int]:
    replacements = 0

    def replace(match: re.Match[str]) -> str:
        nonlocal replacements
        attr = match.group(1)
        value = match.group(2)
        if value in BINDING_MAP:
            replacements += 1
            return f'{attr}="{BINDING_MAP[value]}"'
        return match.group(0)

    updated = BINDING_ATTR_PATTERN.sub(replace, text)
    return updated, replacements


def main() -> int:
    root = Path(__file__).resolve().parent
    var_rows_dir = root / "VarRows"

    if not var_rows_dir.exists():
        print(f"VarRows directory not found at: {var_rows_dir}")
        return 1

    uxml_files = sorted(var_rows_dir.rglob("*.uxml"))
    if not uxml_files:
        print(f"No .uxml files found under: {var_rows_dir}")
        return 0

    total_files = 0
    total_replacements = 0

    for uxml_path in uxml_files:
        original = uxml_path.read_text(encoding="utf-8")
        updated, replacements = update_bindings(original)

        if replacements > 0:
            uxml_path.write_text(updated, encoding="utf-8")
            total_files += 1
            total_replacements += replacements
            print(f"Updated {uxml_path} ({replacements} binding-path updates)")

    print(f"Done. Updated {total_files} file(s), {total_replacements} binding-path replacement(s).")
    return 0


if __name__ == "__main__":
    sys.exit(main())