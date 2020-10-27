#define public Root       "..\.."
#define public BuildType  "Release"
#ifndef VERSION
  #define public VERSION  "v3"
#endif

[Setup]
AppName=Virtual Radar Language Pack
AppVerName=Virtual Radar Language Pack {#VERSION}
DefaultDirName={autopf}\VirtualRadar
DefaultGroupName=Virtual Radar
DisableDirPage=no
InfoBeforeFile=LanguagePack-Contents.rtf
LicenseFile={#Root}\License.txt
; .NET 4.6.1 minimum version is Windows 7 SP1
MinVersion=6.1.7601
OutputBaseFileName=LanguagePack-{#Version}
SetupIconFile={#Root}\VirtualRadar\Application.ico
WizardImageFile=..\Resources\WizardImage.bmp
WizardSmallImageFile=..\Resources\WizardSmallImage.bmp
WizardImageStretch=yes
UninstallDisplayIcon={app}\VirtualRadar.exe

[Messages]
WizardInfoBefore=Contents
InfoBeforeLabel=The languages that can be installed

[Types]
Name: "full"; Description: "All languages";
Name: "chinese"; Description: "Chinese";
Name: "german"; Description: "German";
Name: "french"; Description: "French";                   
Name: "portbr"; Description: "Portuguese (Brazilian)";                   
Name: "russian"; Description: "Russian";
Name: "custom"; Description: "Custom installation"; Flags: iscustom;

[Components]
Name: VrsChinese; Description: "Virtual Radar Server - Chinese (2.2.0)"; Types: full chinese custom;
Name: VrsGerman;  Description: "Virtual Radar Server - German (2.4.0)"; Types: full german custom;
Name: VrsFrench;  Description: "Virtual Radar Server - French (2.2.0)"; Types: full french custom;
Name: VrsPortBr;  Description: "Virtual Radar Server - Portuguese (Brazil) (2.4.0)"; Types: full portbr custom;
Name: VrsRussian; Description: "Virtual Radar Server - Russian (2.4.0)"; Types: full russian custom;
Name: WebChinese; Description: "Web Site Translations - Chinese (2.2.0)"; Types: full chinese custom;
Name: WebGerman;  Description: "Web Site Translations - German (2.4.0)"; Types: full german custom;
Name: WebFrench;  Description: "Web Site Translations - French (2.2.0)"; Types: full french custom;
Name: WebPortBr;  Description: "Web Site Translations - Portuguese (Brazil) (2.4.0)"; Types: full portbr custom;
Name: WebRussian; Description: "Web Site Translations - Russian (2.4.0)"; Types: full russian custom;
Name: DbwChinese; Description: "Database Writer Plugin - Chinese (2.2.0)"; Types: full chinese custom;
Name: DbwGerman;  Description: "Database Writer Plugin - German (2.4.0)"; Types: full german custom;
Name: DbwFrench;  Description: "Database Writer Plugin - French (2.2.0)"; Types: full french custom;
Name: DbwPortBr;  Description: "Database Writer Plugin - Portuguese (Brazil) (2.4.0)"; Types: full portbr custom;
Name: DbwRussian; Description: "Database Writer Plugin - Russian (2.4.0)"; Types: full russian custom;
Name: CcpChinese; Description: "Custom Content Plugin - Chinese (2.2.0)"; Types: full chinese custom;
Name: CcpGerman;  Description: "Custom Content Plugin - German (2.4.0)"; Types: full german custom;
Name: CcpFrench;  Description: "Custom Content Plugin - French (2.2.0)"; Types: full french custom;
Name: CcpPortBr;  Description: "Custom Content Plugin - Portuguese (Brazil) (2.4.0)"; Types: full portbr custom;
Name: CcpRussian; Description: "Custom Content Plugin - Russian (2.4.0)"; Types: full russian custom;
Name: WapChinese; Description: "Web Admin Plugin - Chinese (2.4.0)"; Types: full chinese custom;
Name: WapGerman;  Description: "Web Admin Plugin - German (2.4.0)"; Types: full german custom;
Name: WapFrench;  Description: "Web Admin Plugin - French (2.4.0)"; Types: full french custom;
Name: WapPortBr;  Description: "Web Admin Plugin - Portuguese (Brazil) (2.4.0)"; Types: full portbr custom;
Name: WapRussian; Description: "Web Admin Plugin - Russian (2.4.0)"; Types: full russian custom;


[Files]
; Virtual Radar Server languages
#define VRSLANG(ISOCODE, COMP) "Source: """ + Root + "\VirtualRadar\bin\x86\" + BuildType + "\" + ISOCODE + "\VirtualRadar.Localisation.resources.dll""; DestDir: ""{app}\" + ISOCODE + """; Components: " + COMP + "; Flags: ignoreversion"
#emit VRSLANG("zh-CN", "VrsChinese")
#emit VRSLANG("de-DE", "VrsGerman")
#emit VRSLANG("fr-FR", "VrsFrench")
#emit VRSLANG("pt-BR", "VrsPortBr")
#emit VRSLANG("ru-RU", "VrsRussian")

; Web Site languages
#define WEBLANG(ISOCODE, COMP) "Source: """ + Root + "\VirtualRadar\bin\x86\" + BuildType + "\" + ISOCODE + "\VirtualRadar.WebSite.resources.dll""; DestDir: ""{app}\" + ISOCODE + """; Components: " + COMP + "; Flags: ignoreversion"
#emit WEBLANG("zh-CN", "WebChinese")
#emit WEBLANG("de-DE", "WebGerman")
#emit WEBLANG("fr-FR", "WebFrench")
#emit WEBLANG("pt-BR", "WebPortBr")
#emit WEBLANG("ru-RU", "WebRussian")

; Database Writer Plugin languages
#define DBWLANG(ISOCODE, COMP) "Source: """ + Root + "\Plugin.BaseStationDatabaseWriter\bin\" + BuildType + "\" + ISOCODE + "\VirtualRadar.Plugin.BaseStationDatabaseWriter.resources.dll""; DestDir: ""{app}\" + ISOCODE + """; Components: " + COMP + "; Flags: ignoreversion"
#emit DBWLANG("zh-CN", "DbwChinese")
#emit DBWLANG("de-DE", "DbwGerman")
#emit DBWLANG("fr-FR", "DbwFrench")
#emit DBWLANG("pt-BR", "DbwPortBr")
#emit DBWLANG("ru-RU", "DbwRussian")

; Custom Content Plugin languages
#define CCPLANG(ISOCODE, COMP) "Source: """ + Root + "\Plugin.CustomContent\bin\" + BuildType + "\" + ISOCODE + "\VirtualRadar.Plugin.CustomContent.resources.dll""; DestDir: ""{app}\" + ISOCODE + """; Components: " + COMP + "; Flags: ignoreversion"
#emit CCPLANG("zh-CN", "CcpChinese")
#emit CCPLANG("de-DE", "CcpGerman")
#emit CCPLANG("fr-FR", "CcpFrench")
#emit CCPLANG("pt-BR", "CcpPortBr")
#emit CCPLANG("ru-RU", "CcpRussian")

; Web Admin Plugin languages
#define WAPLANG(ISOCODE, COMP) "Source: """ + Root + "\Plugin.WebAdmin\bin\" + BuildType + "\" + ISOCODE + "\VirtualRadar.Plugin.WebAdmin.resources.dll""; DestDir: ""{app}\" + ISOCODE + """; Components: " + COMP + "; Flags: ignoreversion"
#emit WAPLANG("zh-CN", "WapChinese")
#emit WAPLANG("de-DE", "WapGerman")
#emit WAPLANG("fr-FR", "WapFrench")
#emit WAPLANG("pt-BR", "WapPortBr")
#emit WAPLANG("ru-RU", "WapRussian")
