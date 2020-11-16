@echo off

set                       "ISCC=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if not exist "%ISCC%" set "ISCC=C:\Program Files (x86)\Inno Setup 5\ISCC.exe"

set SRC=%~dp0InnoSetup
set VER=%1

if "%VER%"=="" set VER=v2

for /f %%F in ('dir /b %SRC%\*.iss') do (
    "%ISCC%" /DVERSION=%VER% "%SRC%\%%F"
    if ERRORLEVEL 1 goto :EOF
)

goto :EOF
