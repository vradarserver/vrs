@echo off

rem This should be executed as a part of the post-build step for the service executable.
rem The parameters are:
rem 1) $(TargetDir)
rem 2) $(TargetFileName)
rem 3) $(SolutionDir)
rem 4) $(ConfigurationName)

    set TARGETDIR=%~1
    set FILENAME=%~2
    set SLNDIR=%~3
    set CONFDIR=%~4

    set EXESOURCE=%TARGETDIR%%FILENAME%
    set EXEDEST=%SLNDIR%VirtualRadar\bin\x86\%CONFDIR%\%FILENAME%

    set CONFIGSOURCE=%EXESOURCE%.config
    set CONFIGDEST=%EXEDEST%.config

:CPPLUGIN
    echo Copying "%EXESOURCE%" to "%EXEDEST%"
    copy /Y "%EXESOURCE%" "%EXEDEST%"

    echo Copying "%CONFIGSOURCE%" to "%CONFIGDEST%"
    copy /Y "%CONFIGSOURCE%" "%CONFIGDEST%"

    goto :END


:END