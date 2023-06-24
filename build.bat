@echo off

set "VSCPMSB15=%VSINSTALLDIR%MSBuild\15.0\Bin\msbuild.exe"
set "VSCPMSB=%VSINSTALLDIR%MSBuild\Current\Bin\msbuild.exe"
set "VS2022MSB=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\msbuild.exe"

set                          "MSBUILD=%VSCPMSB15%"
if not exist "%MSBUILD%" set "MSBUILD=%VSCPMSB%"
if not exist "%MSBUILD%" set "MSBUILD=%VS2022MSB%"

set SRC=%~dp0
set "SOLUTION=%SRC%VirtualRadar.sln"
set NOWARN=1570,1572,1573,1574,1584,1587,1591,1711

set DEBUG=0
set RELEASE=0
set X86=0
set X64=0

:NEXTARG
    if "%~1"=="" goto :ENDARGS
    if "%1"=="-release" set RELEASE=1
    if "%1"=="-debug"   set DEBUG=1
    if "%1"=="-vs2022"  set "MSBUILD=%VS2022MSB%"
    shift
    goto :NEXTARG
:ENDARGS

set CONFIGS=""
if %RELEASE%==1 if %DEBUG%==0 set CONFIGS=Release
if %RELEASE%==0 if %DEBUG%==1 set CONFIGS=Debug
if %CONFIGS%==""              set CONFIGS=Release,Debug

set PLATFORMS="Any CPU"

set CONFIG=""
set PLATFORM=""
for %%C IN (%CONFIGS%) do (
    set CONFIG=%%C
    for %%P IN (%PLATFORMS%) do (
        set PLATFORM=%%P

        "%MSBUILD%" "%SOLUTION%" -target:Restore,Build -property:Configuration=%%C,Platform=%%P -nowarn:%NOWARN%
        if ERRORLEVEL 1 goto :FAILED
    )
)

echo.
echo Configurations: %CONFIGS%
echo Platforms:      %PLATFORMS%
echo MSBuild:        %MSBUILD%
echo.
echo Build finished with no errors

goto :EOF

:FAILED
    echo Compilation failed - build is incomplete
    echo Configuration: %CONFIG%
    echo Platform:      %PLATFORM%
    echo MSBuild:       %MSBUILD%
    echo.
    echo Build failed
    exit /b 1
