@echo off

set SRC=%~dp0Mono
if not exist %SRC%\Output\nul goto :EOF
del /q %SRC%\Output\*.*
