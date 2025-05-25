@echo off
setlocal

REM === This script requires 7-Zip CLI (7z.exe) to be installed and in your PATH ===

set "SOURCE=bin/Debug/Civil3D.dll"
set "PASSWORD=123"
set "ZIP1=Civil3D.zip"
set "ZIP2=Civix.zip"

for %%F in ("%SOURCE%") do (
    set "SOURCE_DIR=%%~dpF"
    set "SOURCE_FILE=%%~nxF"
)

if not exist "%SOURCE%" (
    echo Error: Source file "%SOURCE%" not found!
    exit /b 1
)

if exist "%ZIP1%" del "%ZIP1%"
if exist "%ZIP2%" del "%ZIP2%"

pushd "%SOURCE_DIR%"
7z a -tzip "%~dp0%ZIP1%" "%SOURCE_FILE%" -p%PASSWORD% -mem=AES256 -mx=9 -bso0 -bsp0
popd

if errorlevel 1 (
    echo Error: Failed to create %ZIP1%.
    exit /b 1
)

7z a -tzip "%ZIP2%" "%ZIP1%" -p%PASSWORD% -mem=AES256 -mx=9 -bso0 -bsp0

if errorlevel 1 (
    echo Error: Failed to create %ZIP2%.
    if exist "%ZIP1%" del "%ZIP1%"
    exit /b 1
)

del "%ZIP1%"

echo Archive created: %ZIP2%

endlocal
