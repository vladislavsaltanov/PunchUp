@echo off
setlocal

echo [PunchUp] Git setup - Unity SmartMerge
echo.

set "UNITY_VERSION=6000.2.10f1"
set "HUB_EXE=C:\Program Files\Unity Hub\Unity Hub.exe"

if not exist "%HUB_EXE%" (
    echo [ERROR] Unity Hub not found at: %HUB_EXE%
    pause
    exit /b 1
)

:: Get Unity.exe path from Hub, strip the exe, append correct tool path
for /f "delims=" %%P in ('powershell -NoProfile -Command "& 'C:\Program Files\Unity Hub\Unity Hub.exe' -- --headless editors -i 2>$null | Where-Object { $_ -match '%UNITY_VERSION%' } | ForEach-Object { if ($_ -match 'installed at (.+)$') { $p = $matches[1].Trim(); if ($p -match '\.exe$') { Split-Path $p -Parent } else { $p } } }"') do (
    set "EDITOR_DIR=%%P"
)

if not defined EDITOR_DIR (
    echo [ERROR] Unity %UNITY_VERSION% not found via Unity Hub.
    pause
    exit /b 1
)

set "YAML_MERGE=%EDITOR_DIR%\Data\Tools\UnityYAMLMerge.exe"

if not exist "%YAML_MERGE%" (
    echo [ERROR] UnityYAMLMerge.exe not found at:
    echo %YAML_MERGE%
    pause
    exit /b 1
)

echo Found: %YAML_MERGE%
echo.

git config --global merge.unityyamlmerge.name "Unity SmartMerge"
git config --global merge.unityyamlmerge.driver "\"%YAML_MERGE%\" merge -p %%O %%B %%A %%P"
git config --global merge.unityyamlmerge.recursive binary

echo [OK] Unity SmartMerge configured globally.
echo.
pause