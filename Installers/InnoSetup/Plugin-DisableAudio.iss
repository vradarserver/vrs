#define public Root       "..\.."
#define public BuildType  "Release"
#define public Plugin     "{app}\Plugins\DisableAudio"
#ifndef VERSION
  #define public VERSION    "v2"
#endif

[Setup]
AppName=Disable Audio VRS Plugin
AppVerName=Disable Audio VRS Plugin {#VERSION}
DefaultDirName={autopf}\VirtualRadar
DefaultGroupName=Virtual Radar
DisableDirPage=no
InfoBeforeFile=Plugin-DisableAudio-VersionHistory.rtf
LicenseFile={#Root}\License.txt
OutputBaseFileName=Plugin-DisableAudio-{#VERSION}
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
Source: "{#Root}\Plugin.DisableAudio\bin\{#BuildType}\VirtualRadar.Plugin.DisableAudio.dll"; DestDir: "{#Plugin}"; Flags: ignoreversion;

; Manifest file
Source: "{#Root}\Plugin.DisableAudio\bin\{#BuildType}\VirtualRadar.Plugin.DisableAudio.xml"; DestDir: "{#Plugin}"; Flags: ignoreversion;

