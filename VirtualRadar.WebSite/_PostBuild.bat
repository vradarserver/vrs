@echo off

rem This batch file copies the content of the Web folder to a folder called Web under the VirtualRadar
rem build folder. It destroys existing content in the build folder's version of Web.
rem
rem It also builds the Checksums.txt file and copies it into position in the VirtualRadar build folder.
rem
rem The arguments to the batch file are:
rem %1 = $(SolutionDir)
rem %2 = $(ProjectDir)
rem %3 = $(ConfigurationName)

set SOLUTIONDIR=%~1
set PROJECTDIR=%~2
set CONFIGNAME=%~3

rem Build the checksums file directly into the VirtualRadar build folder
set CHKSUMEXE=%SOLUTIONDIR%ThirdParty\ChecksumFiles\bin\%CONFIGNAME%\ChecksumFiles.exe
set CHKSUMROOT=%PROJECTDIR%Site\Web
set CHKSUMFILE=%SOLUTIONDIR%VirtualRadar\bin\x86\%CONFIGNAME%\Checksums.txt
if exist "%CHKSUMEXE%" goto :CHKSUMOK
    echo FAILED: The checksum executable "%CHKSUMEXE%" does not exist.
    goto ENDBAD
:CHKSUMOK
    echo Generating checksums of web content
    echo "%CHKSUMEXE%" -root:"%CHKSUMROOT%" -out:"%CHKSUMFILE%"
    "%CHKSUMEXE%" -root:"%CHKSUMROOT%" -out:"%CHKSUMFILE%" -addContentChecksum
    if not errorlevel 1 goto CHKSUMWORKED
    echo FAILED: Could not generate checksum file "%CHKSUMFILE%", errorlevel is %ERRORLEVEL%
    goto ENDBAD
:CHKSUMWORKED

rem The unit tests on the web site need to have the checksums file in a fixed location, i.e.
rem its folder should not include the configuration name
xcopy /QY "%CHKSUMFILE%" "%PROJECTDIR%Site"

rem Copy the web content into the Web folder under the VirtualRadar build folder
set CPSOURCE=%PROJECTDIR%Site\Web
set CPDEST=%SOLUTIONDIR%VirtualRadar\bin\x86\%CONFIGNAME%\Web

if exist "%CPSOURCE%\" goto SOURCEOK
    echo FAILED: The source folder "%CPSOURCE%" does not exist.
    goto ENDBAD
:SOURCEOK

if not exist "%CPDEST%\" goto DESTMISSING
    echo Erasing "%CPDEST%"
    rmdir /s /q "%CPDEST%"
    if not errorlevel 1 goto DESTMISSING
    echo FAILED: Could not remove the "%CPDEST%" folder, errorlevel is %ERRORLEVEL%
    goto ENDBAD
:DESTMISSING

echo Copying "%CPSOURCE%" to "%CPDEST%"
xcopy /EQYI "%CPSOURCE%" "%CPDEST%"
if not errorlevel 0 goto :BADWEBCOPY
    echo Copied web site output to VirtualRadar build folder
    goto CPOK
:BADWEBCOPY
    echo FAILED: The copy failed with errorlevel %ERRORLEVEL%
    goto ENDBAD
:CPOK

rem Exit points
:ENDGOOD
    exit /b 0

:ENDBAD
    exit /b 1
