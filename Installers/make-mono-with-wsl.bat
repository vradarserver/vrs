@echo off

set SRC=%~dp0Mono
set VER=%1
set CWD=%CD%

cd /d "%SRC%"

if "%VER%"=="" set VER=v3

for /f %%F in ('dir /b *.sh') do (
    wsl "./%%F" %VER%
    if ERRORLEVEL 1 goto :POPCWD
)

:POPCWD
cd /d "%CWD%"
