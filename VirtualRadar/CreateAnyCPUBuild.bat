@echo off

rem %1 = SolutionDir
rem %2 = ProjectDir
rem %3 = ConfigurationName
rem %4 = TargetDir

set SolutionDir=%~1
set ProjectDir=%~2
set ConfigurationName=%~3
set TargetDir=%~4
set CORFLAGS=%SolutionDir%Dependencies\AnyCPUBuilds\CorFlags.exe

set DEST=%ProjectDir%bin\AnyCPU\%ConfigurationName%

if not exist %DEST%\nul goto DOCOPY
    del /s /q %DEST%\*
    for /f "delims=" %%f in ('dir /b %DEST%') do (rmdir /s /q "%DEST%\%%f")

:DOCOPY
xcopy /ei %TargetDir%* %DEST%

rem Remove x86-only files
del /q %DEST%\Microsoft.FlightSimulator.SimConnect.dll
del /q %DEST%\System.Data.SQLite.dll
del /q %DEST%\VirtualRadar.SQLiteWrapper.*

rem Copy in the AnyCPU builds of libraries that can't just have the 32-bit flag switched off
xcopy /eiy %SolutionDir%SQLiteWrapper.DotNet.AnyCPU\bin\%ConfigurationName%\* %DEST%

rem Remove the 32-bit required flag from all VRS DLLs and EXEs
%CORFLAGS% %DEST%\VirtualRadar.exe /32BITREQ- /NOLOGO
%CORFLAGS% %DEST%\VirtualRadar-Service.exe /32BITREQ- /NOLOGO
for /f %%f in ('dir /s/b %DEST%\VirtualRadar.*.dll') do (%CORFLAGS% %%f /32BITREQ- /NOLOGO)

rem Remove the 32-bit required flag from all utilities
%CORFLAGS% %DEST%\BaseStationImport.exe /32BITREQ- /NOLOGO

rem Copy in the AnyCPU interop folders
xcopy /ei %SolutionDir%Dependencies\AnyCPUBuilds\x86 %DEST%\x86
xcopy /ei %SolutionDir%Dependencies\AnyCPUBuilds\x64 %DEST%\x64
