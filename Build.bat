@echo off

set "VSCPMSB=%VSINSTALLDIR%MSBuild\Current\Bin\msbuild.exe"
set "VS2019MSB=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe"
set "VS2022MSB=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\msbuild.exe"
set "VS2026MSB=C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\msbuild.exe"

set                          "MSBUILD=%VSCPMSB%"
if not exist "%MSBUILD%" set "MSBUILD=%VS2026MSB%"
if not exist "%MSBUILD%" set "MSBUILD=%VS2022MSB%"
if not exist "%MSBUILD%" set "MSBUILD=%VS2019MSB%"

set SRC=%~dp0
set "SOLUTION=%SRC%VirtualRadar.sln"
set NOWARN=1570,1572,1573,1574,1584,1587,1591,1711

set DEBUG=0
set RELEASE=0
set X86=0
set X64=0
set RESTORE=0

if "%~1"=="" goto :USAGE

:NEXTARG
    if "%~1"=="" goto :ENDARGS
    if "%1"=="-release" set RELEASE=1
    if "%1"=="-debug"   set DEBUG=1
    if "%1"=="-x86"     set X86=1
    if "%1"=="-x64"     set X64=1
    if "%1"=="-restore" set RESTORE=1
    if "%1"=="-vs2019"  set "MSBUILD=%VS2019MSB%"
    if "%1"=="-vs2022"  set "MSBUILD=%VS2022MSB%"
    if "%1"=="-vs2026"  set "MSBUILD=%VS2026MSB%"
    shift
    goto :NEXTARG
:ENDARGS

if %RESTORE%==1 goto :RESTORE

set CONFIGS=""
if %RELEASE%==1 if %DEBUG%==0 set CONFIGS=Release
if %RELEASE%==0 if %DEBUG%==1 set CONFIGS=Debug
if %CONFIGS%==""              set CONFIGS=Release,Debug

set PLATFORMS=""
if %X86%==1 if %X64%==0 set PLATFORMS=x86
if %X86%==0 if %X64%==1 set PLATFORMS=x64
if %PLATFORMS%==""      set PLATFORMS=x86,x64

set CONFIG=""
set PLATFORM=""
for %%C IN (%CONFIGS%) do (
    set CONFIG=%%C
    for %%P IN (%PLATFORMS%) do (
        set PLATFORM=%%P

        "%MSBUILD%" "%SOLUTION%" -restore -target:Build -property:RestorePackagesConfig=true -property:Configuration=%%C,Platform=%%P -nowarn:%NOWARN%
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

:RESTORE
    "%MSBUILD%" "%SOLUTION%" -target:Restore -property:RestorePackagesConfig=true
    if ERRORLEVEL 1 goto :RESTOREFAILED
    echo.
    echo MSBuild:        %MSBUILD%
    echo.
    echo Restore finished with no errors
    goto :EOF

:RESTOREFAILED
    echo MSBuild:       %MSBUILD%
    echo.
    echo Restore failed
    exit /b 1

:USAGE
    echo usage: Build.bat [-debug] [-release] [-x86] [-x64] [-vs2019 ^| -vs2022 ^| -vs2026]
    echo        Build.bat -restore [-vs2019 ^| -vs2022 ^| -vs2026]
    echo.
    echo   -restore    Restore NuGet packages without building
    echo   -debug      Build the Debug configuration
    echo   -release    Build the Release configuration
    echo   -x86        Build the x86 platform
    echo   -x64        Build the x64 platform
    echo   -vs2019     Use the Visual Studio 2019 MSBuild
    echo   -vs2022     Use the Visual Studio 2022 MSBuild
    echo   -vs2026     Use the Visual Studio 2026 MSBuild
    echo.
    echo If no configuration is specified then both configurations are built.
    echo If no platform is specified then both platforms are built.
    echo Without a -vs switch MSBuild is taken from the Visual Studio developer prompt,
    echo otherwise from Visual Studio 2026, 2022 or 2019, whichever is found first.
    exit /b 1

:FAILED
    echo Compilation failed - build is incomplete
    echo Configuration: %CONFIG%
    echo Platform:      %PLATFORM%
    echo MSBuild:       %MSBUILD%
    echo.
    echo Build failed
    exit /b 1
