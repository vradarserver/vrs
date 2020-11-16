@echo off

cmd /c %~dp0make-innosetup.bat
if ERRORLEVEL 1 goto :EOF

cmd /c %~dp0make-mono-with-wsl.bat
if ERRORLEVEL 1 goto :EOF
