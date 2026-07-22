@echo off
echo ===================================
echo   PesDuke Build Script
echo ===================================
echo.

set PATH=C:\dotnet;%PATH%

echo [1/3] Building self-contained EXE...
C:\dotnet\dotnet.exe publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o C:\Users\fante\Documents\PesDuke\publish
if %ERRORLEVEL% NEQ 0 (
    echo BUILD FAILED!
    pause
    exit /b 1
)
echo EXE built successfully: publish\PesDuke.exe
echo.

echo [2/3] Checking for Inno Setup...
where iscc >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [3/3] Building installer...
    mkdir installer\output 2>nul
    iscc installer\pesduke_setup.iss
    echo Installer built: installer\output\PesDuke-Setup-v1.0.exe
) else (
    echo [3/3] Inno Setup not found - skipping installer
    echo Download from: https://jrsoftware.org/isinfo.php
    echo Or install with: winget install JRSoftware.InnoSetup
)

echo.
echo ===================================
echo   BUILD COMPLETE
echo ===================================
echo EXE: publish\PesDuke.exe
pause
