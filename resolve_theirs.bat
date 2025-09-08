@echo off
REM Resolve all merge conflicts using "theirs" version

REM Make sure we’re in a Git repo
git rev-parse --is-inside-work-tree >nul 2>&1
if errorlevel 1 (
    echo Not inside a Git repository.
    exit /b 1
)

REM Find all conflicted files and check them out from "theirs"
for /f "delims=" %%f in ('git diff --name-only --diff-filter=U') do (
    echo Resolving %%f using "theirs"...
    git checkout --theirs -- "%%f"
    git add "%%f"
)

echo.
echo All conflicts resolved using "theirs".
echo You can now run: git commit