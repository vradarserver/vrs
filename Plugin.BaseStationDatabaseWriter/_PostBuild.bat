@echo off

rem This should be executed as a part of the post-build step for the plugin.
rem The parameters are:
rem 1) $(SolutionDir)
rem 2) $(ConfigurationName)
rem 3) $(TargetDir)
rem 4) $(TargetName)
rem 5) Name-Of-Plugin-SubFolder
rem Ensure that the XML manifest file is set to always copy to the output directory on a build

    if "%5"=="" goto NOPARAM

    set SOURCEDLL=%3\%4.dll
    set SOURCEXML=%3\%4.xml
    set PLUGINS=%1VirtualRadar\bin\x86\%2\Plugins
    set DEST=%PLUGINS%\%5

    if exist "%PLUGINS%" goto MKDEST
        md "%PLUGINS%"

:MKDEST
    if exist "%DEST%" goto CPPLUGIN
        md "%DEST%"

:CPPLUGIN
    copy "%SOURCEDLL%" "%DEST%"
    copy "%SOURCEXML%" "%DEST%"

    rem You need to modify the batch by hand here to copy the languages that you have translations for
    set COPYLANG=%~3..\..\..\_PostBuildCopyLanguage.bat
    call %COPYLANG% "%1" "%2" "%3" "%4" de-DE
    call %COPYLANG% "%1" "%2" "%3" "%4" fr-FR
    call %COPYLANG% "%1" "%2" "%3" "%4" ru-RU

    goto :END

:NOPARAM
    echo.
    echo Missing parameter - add this as a post-build step but remember to replace the
    echo "YOUR_PLUGIN_NAME_HERE" part with the name of your plugin:
    echo.
    echo $(ProjectDir)\_PostBuild.bat "$(SolutionDir)" "$(ConfigurationName)" "$(TargetPath)" "YOUR_PLUGIN_NAME_HERE"
    echo.

:END