#define public Root       "..\.."
#define public BuildType  "Release"
#define public Plugin     "{app}\Plugins\FeedFilter"
#ifndef VERSION
  #define public VERSION    "v3"
#endif

[Setup]
AppName=Feed Filter VRS Plugin
AppVerName=Feed FilterVRS Plugin {#VERSION}
DefaultDirName={autopf}\VirtualRadar
DefaultGroupName=Virtual Radar
DisableDirPage=no
InfoBeforeFile=Plugin-FeedFilter-VersionHistory.rtf
LicenseFile={#Root}\License.txt
; .NET 4.6.1 minimum version is Windows 7 SP1
MinVersion=6.1.7601
OutputBaseFileName=Plugin-FeedFilter-{#VERSION}
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
Source: "{#Root}\Plugin.FeedFilter\bin\Release\VirtualRadar.Plugin.FeedFilter.dll"; DestDir: "{#Plugin}"; Flags: ignoreversion;

; Manifest file
Source: "{#Root}\Plugin.FeedFilter\VirtualRadar.Plugin.FeedFilter.xml"; DestDir: "{#Plugin}"; Flags: ignoreversion;

; Web site files
Source: "{#Root}\Plugin.FeedFilter\Web\*"; DestDir: "{#Plugin}\Web"; Excludes: "zz-norel-*"; Flags: ignoreversion recursesubdirs;
Source: "{#Root}\Plugin.FeedFilter\Web-WebAdmin\*"; DestDir: "{#Plugin}\Web-WebAdmin"; Excludes: "zz-norel-*"; Flags: ignoreversion recursesubdirs;

[InstallDelete]
; Old web site files
Type: filesandordirs; Name: "{#Plugin}\Web";
Type: filesandordirs; Name: "{#Plugin}\Web-WebAdmin";
