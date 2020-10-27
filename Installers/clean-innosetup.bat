@echo off

set SRC=%~dp0InnoSetup
if not exist %SRC%\Output\nul goto :EOF
del /q %SRC%\Output\*.*
