#define public Root       "..\.."
#define public BuildType  "Release"
#define public Plugin     "{app}\Plugins\BaseStationDatabaseWriter"
#ifndef VERSION
  #define public VERSION    "v2"
#endif

[Setup]
AppName=Database Writer VRS Plugin
AppVerName=Database Writer VRS Plugin {#VERSION}
DefaultDirName={autopf}\VirtualRadar
DefaultGroupName=Virtual Radar
DisableDirPage=no
InfoBeforeFile=Plugin-DatabaseWriter-VersionHistory.rtf
LicenseFile={#Root}\License.txt
OutputBaseFileName=Plugin-DatabaseWriter-{#VERSION}
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
Source: "{#Root}\Plugin.BaseStationDatabaseWriter\bin\{#BuildType}\VirtualRadar.Plugin.BaseStationDatabaseWriter.dll"; DestDir: "{#Plugin}"; Flags: ignoreversion;

; Web site files
Source: "{#Root}\Plugin.BaseStationDatabaseWriter\Web\*"; DestDir: "{#Plugin}\Web"; Excludes: "zz-norel-*"; Flags: ignoreversion recursesubdirs;

; Manifest file
Source: "{#Root}\Plugin.BaseStationDatabaseWriter\VirtualRadar.Plugin.BaseStationDatabaseWriter.xml"; DestDir: "{#Plugin}"; Flags: ignoreversion;

[InstallDelete]
; Old web site files
Type: filesandordirs; Name: "{#Plugin}\Web";
