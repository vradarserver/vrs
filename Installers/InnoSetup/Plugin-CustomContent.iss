#define public Root       "..\.."
#define public BuildType  "Release"
#define public Plugin     "{app}\Plugins\CustomContent"
#ifndef VERSION
  #define public VERSION    "v2"
#endif

[Setup]
AppName=Custom Content VRS Plugin
AppVerName=Custom Content VRS Plugin {#VERSION}
DefaultDirName={autopf}\VirtualRadar
DefaultGroupName=Virtual Radar
DisableDirPage=no
InfoBeforeFile=Plugin-CustomContent-VersionHistory.rtf
LicenseFile={#Root}\License.txt
OutputBaseFileName=Plugin-CustomContent-{#VERSION}
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
Source: "{#Root}\Plugin.CustomContent\bin\{#BuildType}\VirtualRadar.Plugin.CustomContent.dll"; DestDir: "{#Plugin}"; Flags: ignoreversion;

; Web site files
Source: "{#Root}\Plugin.CustomContent\Web\*"; DestDir: "{#Plugin}\Web"; Excludes: "zz-norel-*"; Flags: ignoreversion recursesubdirs;

; Manifest file
Source: "{#Root}\Plugin.CustomContent\VirtualRadar.Plugin.CustomContent.xml"; DestDir: "{#Plugin}"; Flags: ignoreversion;

[InstallDelete]
; Old web site files
Type: filesandordirs; Name: "{#Plugin}\Web";
