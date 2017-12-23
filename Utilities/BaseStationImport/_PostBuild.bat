@echo off

rem This should be executed as a part of the post-build step for the utility.
rem The parameters are:
rem 1) $(SolutionDir)
rem 2) $(ConfigurationName)
rem 3) $(TargetDir)
rem 4) $(TargetName)

    set SLNDIR=%~1
    set CONFDIR=%~2
    set TARGETDIR=%~3
    set ASSEMBLY=%~4

    if "%ASSEMBLY%"=="" goto NOPARAM

    set SOURCEEXE=%TARGETDIR%%ASSEMBLY%.exe
    set SOURCEPDB=%TARGETDIR%%ASSEMBLY%.pdb
    set DEST=%SLNDIR%VirtualRadar\bin\x86\%CONFDIR%

    copy "%SOURCEEXE%" "%DEST%"
    copy "%SOURCEEXE%.config" "%DEST%"
    copy "%SOURCEPDB%" "%DEST%"
    goto :END

:NOPARAM
    echo.
    echo Missing parameter:
    echo.
    echo $(ProjectDir)\_PostBuild.bat "$(SolutionDir)" "$(ConfigurationName)" "$(TargetDir)" "$(TargetName)"
    echo.

:ENDBAD
    exit /b 1

:END
    exit /b 0
