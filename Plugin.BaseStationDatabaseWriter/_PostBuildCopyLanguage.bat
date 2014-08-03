@echo off

rem This copies a single language resource file into the VirtualRadar build folder
rem The parameters are:
rem 1) $(SolutionDir)
rem 2) $(ConfigurationName)
rem 3) $(TargetDir)
rem 4) $(TargetName)
rem 5) Language folder name (e.g. fr-FR)
rem It is intended that this be called from _PostBuild.bat so there isn't any parameter validation

    set SLNDIR=%~1
    set CONFDIR=%~2
    set TARGETDIR=%~3
    set ASSEMBLY=%~4
    set LANG=%~5

    set SOURCEDLL=%TARGETDIR%%LANG%\%ASSEMBLY%.resources.dll
    set DEST=%SLNDIR%VirtualRadar\bin\x86\%CONFDIR%\%LANG%

:MKDEST
    if exist "%DEST%" goto CPLANG
        md "%DEST%"

:CPLANG
    copy "%SOURCEDLL%" "%DEST%"
    goto :END

:END