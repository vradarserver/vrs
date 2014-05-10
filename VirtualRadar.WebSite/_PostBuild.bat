@echo off

rem This batch file copies the content of the Web folder to a folder called Web under the VirtualRadar
rem build folder. It destroys existing content in the build folder's version of Web.
rem
rem The arguments to the batch file are:
rem %1 = Full path to Web folder in VirtualRadar.WebSite
rem %2 = Full path to Web folder in VirtualRadar

set SOURCE=%~1
set DEST=%~2

if exist "%SOURCE%\" goto SOURCEOK
    echo FAILED: The source folder "%SOURCE%" does not exist.
    goto ENDBAD
:SOURCEOK

if not exist "%DEST%\" goto DESTMISSING
    echo Erasing "%DEST%"
    rmdir /s /q "%DEST%"
    if errorlevel 0 goto DESTMISSING
    echo FAILED: Could not remove the "%DEST%" folder, errorlevel is %ERRORLEVEL%
    goto ENDBAD
:DESTMISSING

echo Copying "%SOURCE%" to "%DEST%"
xcopy /EQYI "%SOURCE%" "%DEST%"
if not errorlevel 0 goto :FAILED
    echo Copied web site output to VirtualRadar build folder
    goto ENDGOOD
:FAILED
    echo FAILED: The copy failed with errorlevel %ERRORLEVEL%
    goto ENDBAD

:ENDGOOD
    exit /b 0

:ENDBAD
    exit /b 1
