#define public Root       "..\.."
#define public BuildType  "Release"
#define public Plugin     "{app}\Plugins\DisableUPnP"
#ifndef VERSION
  #define public VERSION    "v3"
#endif

[Setup]
AppName=Disable UPnP VRS Plugin
AppVerName=Disable UPnP VRS Plugin {#VERSION}
DefaultDirName={autopf}\VirtualRadar
DefaultGroupName=Virtual Radar
DisableDirPage=no
InfoBeforeFile=Plugin-DisableUPnP-VersionHistory.rtf
LicenseFile={#Root}\License.txt
; .NET 4.6.1 minimum version is Windows 7 SP1
MinVersion=6.1.7601
OutputBaseFileName=Plugin-DisableUPnP-{#VERSION}
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
Source: "{#Root}\Plugin.DisableUPnP\bin\Release\VirtualRadar.Plugin.DisableUPnP.dll"; DestDir: "{#Plugin}"; Flags: ignoreversion;

; Manifest file
Source: "{#Root}\Plugin.DisableUPnP\bin\Release\VirtualRadar.Plugin.DisableUPnP.xml"; DestDir: "{#Plugin}"; Flags: ignoreversion;
