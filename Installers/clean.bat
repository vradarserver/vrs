@echo off

cmd /c %~dp0clean-innosetup.bat
if ERRORLEVEL 1 goto :EOF

cmd /c %~dp0clean-mono.bat
if ERRORLEVEL 1 goto :EOF
