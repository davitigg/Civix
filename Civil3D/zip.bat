@echo off
setlocal

REM === This script requires 7-Zip CLI (7z.exe) to be installed and in your PATH ===

set "SOURCE=bin/Debug/Civil3D.dll"
set "PASSWORD=123"
set "ARCH1=Civil3D.7z"
set "ARCH2=Civix.7z"

for %%F in ("%SOURCE%") do (
    set "SOURCE_DIR=%%~dpF"
    set "SOURCE_FILE=%%~nxF"
)

if not exist "%SOURCE%" (
    echo Error: Source file "%SOURCE%" not found!
    exit /b 1
)

if exist "%ARCH1%" del "%ARCH1%"
if exist "%ARCH2%" del "%ARCH2%"

pushd "%SOURCE_DIR%"
7z a "%~dp0%ARCH1%" "%SOURCE_FILE%" -p%PASSWORD% -bso0 -bsp0
popd

if errorlevel 1 (
    echo Error: Failed to create %ARCH1%.
    exit /b 1
)

7z a "%ARCH2%" "%ARCH1%" -p%PASSWORD% -bso0 -bsp0

if errorlevel 1 (
    echo Error: Failed to create %ARCH2%.
    if exist "%ARCH1%" del "%ARCH1%"
    exit /b 1
)

del "%ARCH1%"

echo Archive created: %ARCH2%

endlocal
