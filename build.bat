@echo off
REM Build script for FMHY Free Movies Jellyfin Plugin (Windows)

set PROJECT_DIR=FmhyPlugin
set OUTPUT_DIR=output
set PLUGIN_NAME=FmhyPlugin

echo === FMHY Free Movies Plugin Build Script ===
echo.

REM Clean previous builds
echo Cleaning previous builds...
if exist "%OUTPUT_DIR%" rmdir /s /q "%OUTPUT_DIR%"
if exist "%PROJECT_DIR%\bin" rmdir /s /q "%PROJECT_DIR%\bin"
if exist "%PROJECT_DIR%\obj" rmdir /s /q "%PROJECT_DIR%\obj"

REM Create output directory
mkdir "%OUTPUT_DIR%"

REM Restore dependencies
echo Restoring dependencies...
cd "%PROJECT_DIR%"
dotnet restore

REM Build release
echo Building release...
dotnet build -c Release --no-restore

REM Create plugin package
echo Creating plugin package...
cd ..
mkdir "%OUTPUT_DIR%\%PLUGIN_NAME%"

REM Copy DLL
copy "%PROJECT_DIR%\bin\Release\net8.0\%PLUGIN_NAME%.dll" "%OUTPUT_DIR%\%PLUGIN_NAME%\"

REM Copy plugin.json
copy "%PROJECT_DIR%\plugin.json" "%OUTPUT_DIR%\%PLUGIN_NAME%\"

REM Copy web files
mkdir "%OUTPUT_DIR%\%PLUGIN_NAME%\Web"
copy "%PROJECT_DIR%\Web\configuration.html" "%OUTPUT_DIR%\%PLUGIN_NAME%\Web\"
copy "%PROJECT_DIR%\Web\browse.html" "%OUTPUT_DIR%\%PLUGIN_NAME%\Web\"

REM Create zip (requires PowerShell 5+)
echo Creating zip package...
cd "%OUTPUT_DIR%"
powershell -Command "Compress-Archive -Path '%PLUGIN_NAME%' -DestinationPath '%PLUGIN_NAME%.zip' -Force"
cd ..

echo.
echo === Build Complete ===
echo Plugin package: %OUTPUT_DIR%\%PLUGIN_NAME%.zip
echo Extracted plugin: %OUTPUT_DIR%\%PLUGIN_NAME%\
echo.
echo To install:
echo 1. Extract %OUTPUT_DIR%\%PLUGIN_NAME%.zip to your Jellyfin plugins directory
echo 2. Restart Jellyfin
echo 3. Configure at Dashboard ^> Plugins ^> FMHY Free Movies