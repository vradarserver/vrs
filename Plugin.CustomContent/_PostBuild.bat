@echo off

rem This should be executed as a part of the post-build step for the plugin.
rem The parameters are:
rem 1) $(SolutionDir)
rem 2) $(ConfigurationName)
rem 3) $(TargetDir)
rem 4) $(TargetName)
rem 5) Name-Of-Plugin-SubFolder
rem Ensure that the XML manifest file is set to always copy to the output directory on a build

    set SLNDIR=%~1
    set CONFDIR=%~2
    set TARGETDIR=%~3
    set ASSEMBLY=%~4
    set PLUGINDIR=%~5

    if "%PLUGINDIR%"=="" goto NOPARAM

    set SOURCEDLL=%TARGETDIR%%ASSEMBLY%.dll
    set SOURCEXML=%TARGETDIR%%ASSEMBLY%.xml
    set PLUGINS=%SLNDIR%VirtualRadar\bin\x86\%CONFDIR%\Plugins
    set DEST=%PLUGINS%\%PLUGINDIR%

    if exist "%PLUGINS%" goto MKDEST
        md "%PLUGINS%"

:MKDEST
    if exist "%DEST%" goto CPPLUGIN
        md "%DEST%"

:CPPLUGIN
    copy "%SOURCEDLL%" "%DEST%"
    copy "%SOURCEXML%" "%DEST%"

    rem You need to modify the batch by hand here to copy the languages that you have translations for
    set COPYLANG=%TARGETDIR%..\..\_PostBuildCopyLanguage.bat
    call "%COPYLANG%" "%SLNDIR%" "%CONFDIR%" "%TARGETDIR%" "%ASSEMBLY%" de-DE
    call "%COPYLANG%" "%SLNDIR%" "%CONFDIR%" "%TARGETDIR%" "%ASSEMBLY%" fr-FR
    call "%COPYLANG%" "%SLNDIR%" "%CONFDIR%" "%TARGETDIR%" "%ASSEMBLY%" pt-BR
    call "%COPYLANG%" "%SLNDIR%" "%CONFDIR%" "%TARGETDIR%" "%ASSEMBLY%" ru-RU
    call "%COPYLANG%" "%SLNDIR%" "%CONFDIR%" "%TARGETDIR%" "%ASSEMBLY%" zh-CN

    goto :END

:NOPARAM
    echo.
    echo Missing parameter - add this as a post-build step but remember to replace the
    echo "YOUR_PLUGIN_NAME_HERE" part with the name of your plugin:
    echo.
    echo $(ProjectDir)\_PostBuild.bat "$(SolutionDir)" "$(ConfigurationName)" "$(TargetPath)" "YOUR_PLUGIN_NAME_HERE"
    echo.

:END