#define public Root       "..\.."
#define public BuildType  "Release"
#define public Plugin     "{app}\Plugins\DatabaseEditor"
#ifndef VERSION
  #define public VERSION    "v2"
#endif

[Setup]
AppName=Database Editor VRS Plugin
AppVerName=Database Editor VRS Plugin {#VERSION}
DefaultDirName={autopf}\VirtualRadar
DefaultGroupName=Virtual Radar
DisableDirPage=no
InfoBeforeFile=Plugin-DatabaseEditor-VersionHistory.rtf
LicenseFile={#Root}\License.txt
OutputBaseFileName=Plugin-DatabaseEditor-{#VERSION}
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
Source: "{#Root}\Plugin.DatabaseEditor\bin\{#BuildType}\VirtualRadar.Plugin.DatabaseEditor.dll"; DestDir: "{#Plugin}"; Flags: ignoreversion;

; Manifest file
Source: "{#Root}\Plugin.DatabaseEditor\VirtualRadar.Plugin.DatabaseEditor.xml"; DestDir: "{#Plugin}"; Flags: ignoreversion;

; Web site files
Source: "{#Root}\Plugin.DatabaseEditor\Web\*"; DestDir: "{#Plugin}\Web"; Excludes: "zz-norel-*"; Flags: ignoreversion recursesubdirs;
Source: "{#Root}\Plugin.DatabaseEditor\Web-WebAdmin\*"; DestDir: "{#Plugin}\Web-WebAdmin"; Excludes: "zz-norel-*"; Flags: ignoreversion recursesubdirs;

[InstallDelete]
; Old web site files
Type: filesandordirs; Name: "{#Plugin}\Web";
Type: filesandordirs; Name: "{#Plugin}\Web-WebAdmin";
