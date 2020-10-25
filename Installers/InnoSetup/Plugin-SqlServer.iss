#define public Root       "..\.."
#define public BuildType  "Release"
#define public Plugin     "{app}\Plugins\SqlServer"
#ifndef VERSION
  #define public VERSION    "0.0.0"
#endif

[Setup]
AppName=SQL Server VRS Plugin
AppVerName=SQL Server VRS Plugin {#VERSION}
DefaultDirName={autopf}\VirtualRadar
DefaultGroupName=Virtual Radar
DisableDirPage=no
InfoBeforeFile=Plugin-SqlServer-VersionHistory.rtf
LicenseFile={#Root}\License.txt
; .NET 4.6.1 minimum version is Windows 7 SP1
MinVersion=6.1.7601
OutputBaseFileName=Plugin-SqlServer-{#VERSION}
SetupIconFile={#Root}\VirtualRadar\Application.ico
WizardImageFile=..\Resources\WizardImage.bmp
WizardSmallImageFile=..\Resources\WizardSmallImage.bmp

[Messages]
WizardInfoBefore=Version History
InfoBeforeLabel=What has changed?

[Files]
; License
Source: "{#Root}\LICENSE.txt"; DestDir: "{#Plugin}"; Flags: ignoreversion;

; Application files
Source: "{#Root}\Plugin.SqlServer\bin\Release\VirtualRadar.Plugin.SqlServer.dll"; DestDir: "{#Plugin}"; Flags: ignoreversion;

; Web site files
Source: "{#Root}\Plugin.SqlServer\Web\*"; DestDir: "{#Plugin}\Web"; Excludes: "zz-norel-*"; Flags: ignoreversion recursesubdirs;

; Manifest file and schemas
Source: "{#Root}\Plugin.SqlServer\VirtualRadar.Plugin.SqlServer.xml"; DestDir: "{#Plugin}"; Flags: ignoreversion;
Source: "{#Root}\Plugin.SqlServer\Scripts\UpdateSchema.sql"; DestDir: "{#Plugin}"; Flags: ignoreversion;

[InstallDelete]
; Old web site files
Type: filesandordirs; Name: "{#Plugin}\Web";
