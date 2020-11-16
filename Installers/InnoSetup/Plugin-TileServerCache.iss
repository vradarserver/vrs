#define public Root       "..\.."
#define public BuildType  "Release"
#define public Plugin     "{app}\Plugins\TileServerCache"
#ifndef VERSION
  #define public VERSION    "v3"
#endif

[Setup]
AppName=Tile Server Cache VRS Plugin
AppVerName=Tile Server Cache VRS Plugin {#VERSION}
DefaultDirName={autopf}\VirtualRadar
DefaultGroupName=Virtual Radar
DisableDirPage=no
InfoBeforeFile=Plugin-TileServerCache-VersionHistory.rtf
LicenseFile={#Root}\License.txt
; .NET 4.6.1 minimum version is Windows 7 SP1
MinVersion=6.1.7601
OutputBaseFileName=Plugin-TileServerCache-{#VERSION}
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
Source: "{#Root}\Plugin.TileServerCache\bin\{#BuildType}\VirtualRadar.Plugin.TileServerCache.dll"; DestDir: "{#Plugin}"; Flags: ignoreversion;

; Web site files
Source: "{#Root}\Plugin.TileServerCache\Web\*"; DestDir: "{#Plugin}\Web"; Excludes: "zz-norel-*"; Flags: ignoreversion recursesubdirs;

; Manifest file
Source: "{#Root}\Plugin.TileServerCache\VirtualRadar.Plugin.TileServerCache.xml"; DestDir: "{#Plugin}"; Flags: ignoreversion;

[InstallDelete]
; Old web site files
Type: filesandordirs; Name: "{#Plugin}\Web";
