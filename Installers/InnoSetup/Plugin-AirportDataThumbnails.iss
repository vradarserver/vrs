#define public Root       "..\.."
#define public BuildType  "Release"
#define public Plugin     "{app}\Plugins\AirportDataThumbnails"
#ifndef VERSION
  #define public VERSION    "v2"
#endif

[Setup]
AppName=Airport Data Thumbnails VRS Plugin
AppVerName=Airport Data Thumbnails VRS Plugin {#VERSION}
DefaultDirName={autopf}\VirtualRadar
DefaultGroupName=Virtual Radar
DisableDirPage=no
InfoBeforeFile=Plugin-AirportDataThumbnails-VersionHistory.rtf
LicenseFile={#Root}\License.txt
OutputBaseFileName=Plugin-AirportDataThumbnails-{#VERSION}
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
Source: "{#Root}\Plugin.AirportDataThumbnails\bin\{#BuildType}\VirtualRadar.Plugin.AirportDataThumbnails.dll"; DestDir: "{#Plugin}"; Flags: ignoreversion;

; Manifest file
Source: "{#Root}\Plugin.AirportDataThumbnails\bin\{#BuildType}\VirtualRadar.Plugin.AirportDataThumbnails.xml"; DestDir: "{#Plugin}"; Flags: ignoreversion;

; Web files
Source: "{#Root}\Plugin.AirportDataThumbnails\Web\*"; DestDir: "{#Plugin}\Web"; Excludes: "zz-norel-*"; Flags: ignoreversion recursesubdirs;

