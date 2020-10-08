@echo off

set                        "MSBUILD=C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\msbuild.exe"
if not exist "%MSBUILD%" set "MSBUILD=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe"
set SRC=%~dp0
set "SOLUTION=%SRC%VirtualRadar.sln"
set NOWARN=1570,1572,1573,1574,1584,1587,1591,1711

set CONFIG=""
set PLATFORM=""
for %%C IN (Release,Debug) do (
    set CONFIG=%%C
    for %%P IN (x86,x64) do (
        set PLATFORM=%%P

        "%MSBUILD%" "%SOLUTION%" -target:Restore,Build -property:Configuration=%%C,Platform=%%P -nowarn:%NOWARN%
        if ERRORLEVEL 1 goto :FAILED
    )
)

goto :EOF

:FAILED
    echo Compilation failed - build is incomplete
    echo Configuration: %CONFIG%
    echo Platform:      %PLATFORM%
    exit /b 1
