#define public Root      "..\.."
#define public BuildType "Release"
#ifndef VERSION
  #define public VERSION    "0.0.0"
#endif

[Setup]
ArchitecturesInstallIn64BitMode=x64
AppName=Virtual Radar x64
AppVerName=Virtual Radar x64 {#VERSION}
DefaultDirName={autopf}\VirtualRadar
DefaultGroupName=Virtual Radar
DisableDirPage=no
InfoBeforeFile=VirtualRadar-VersionHistory.rtf
LicenseFile={#Root}\License.txt
; .NET 4.6.1 minimum version is Windows 7 SP1
MinVersion=6.1.7601
OutputBaseFileName=VirtualRadarSetup-x64-{#VERSION}
SetupIconFile={#Root}\VirtualRadar\Application.ico
WizardImageFile=..\Resources\WizardImage.bmp
WizardSmallImageFile=..\Resources\WizardSmallImage.bmp
WizardImageStretch=yes
UninstallDisplayIcon={app}\VirtualRadar.exe

[Messages]
WizardInfoBefore=Version History
InfoBeforeLabel=What has changed?

[Tasks]
Name: AddToFirewall; Description: "Configure Windows Firewall so other computers on your network can access Virtual Radar Server"; Flags: unchecked;

[Files]
; License
Source: "{#Root}\LICENSE.txt"; DestDir: "{app}"; Flags: ignoreversion;

; Application files
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\VirtualRadar.exe"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\VirtualRadar.exe.config"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\VirtualRadar-Service.exe"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\VirtualRadar-Service.exe.config"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\VirtualRadar.Database.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\VirtualRadar.Headless.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\VirtualRadar.Interface.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\VirtualRadar.Interop.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\VirtualRadar.Library.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\VirtualRadar.Localisation.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\VirtualRadar.Resources.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\VirtualRadar.SQLiteWrapper.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\VirtualRadar.WebServer.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\VirtualRadar.WebServer.HttpListener.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\VirtualRadar.WebSite.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\VirtualRadar.WinForms.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\InterfaceFactory.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\AWhewell.Owin.Interface.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\AWhewell.Owin.Interface.Host.HttpListener.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\AWhewell.Owin.Interface.WebApi.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\AWhewell.Owin.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\AWhewell.Owin.Host.HttpListener.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\AWhewell.Owin.Utility.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\AWhewell.Owin.WebApi.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\Interop.NATUPNPLib.dll"; DestDir: "{app}"; Flags: ignoreversion;

; Command-line utility files
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\BaseStationImport.exe"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\BaseStationImport.exe.config"; DestDir: "{app}"; Flags: ignoreversion;

; Web site files
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\Web\*"; DestDir: "{app}\Web"; Excludes: "zz-norel-*"; Flags: ignoreversion recursesubdirs;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\Checksums.txt"; DestDir: "{app}"; Flags: ignoreversion;

; Web site translations
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\de-DE\VirtualRadar.WebSite.resources.dll"; DestDir: "{app}\de-DE"; Excludes: "zz-norel-*"; Flags: ignoreversion recursesubdirs;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\fr-FR\VirtualRadar.WebSite.resources.dll"; DestDir: "{app}\fr-FR"; Excludes: "zz-norel-*"; Flags: ignoreversion recursesubdirs;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\pt-BR\VirtualRadar.WebSite.resources.dll"; DestDir: "{app}\pt-BR"; Excludes: "zz-norel-*"; Flags: ignoreversion recursesubdirs;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\ru-RU\VirtualRadar.WebSite.resources.dll"; DestDir: "{app}\ru-RU"; Excludes: "zz-norel-*"; Flags: ignoreversion recursesubdirs;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\zh-CN\VirtualRadar.WebSite.resources.dll"; DestDir: "{app}\zh-CN"; Excludes: "zz-norel-*"; Flags: ignoreversion recursesubdirs;

; SQLite
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\System.Data.SQLite.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\x64\SQLite.Interop.dll"; DestDir: "{app}\x64"; Flags: ignoreversion
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\x86\SQLite.Interop.dll"; DestDir: "{app}\x86"; Flags: ignoreversion

; 3rd party libraries
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\AjaxMin.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\Dapper.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\HtmlAgilityPack.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\KdTreeLib.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion;

; Flight Simulator
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\Microsoft.FlightSimulator.SimConnect.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\SimConnect.cfg"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Root}\VirtualRadar\bin\x64\{#BuildType}\SimConnect.dll"; DestDir: "{app}"; Flags: ignoreversion;

[Dirs]
Name: "{app}\Plugins"
Name: "{localappdata}\VirtualRadar"; AfterInstall: WriteInstallerConfiguration;

[InstallDelete]
; Old web site files
Type: filesandordirs; Name: "{app}\Web";

; Old translations
Type: filesandordirs; Name: "{app}\de-DE";
Type: filesandordirs; Name: "{app}\fr-FR";
Type: filesandordirs; Name: "{app}\pl-PL";
Type: filesandordirs; Name: "{app}\pt-BR";
Type: filesandordirs; Name: "{app}\ru-RU";
Type: filesandordirs; Name: "{app}\zh-CN";

; Files that were laid down by old versions of the installer but are either no longer required or are no longer supported
Type: files; Name: "{app}\DecompressMessageLog.exe"
Type: files; Name: "{app}\IQToolkit.dll"
Type: files; Name: "{app}\IQToolkit.Data.dll"
Type: files; Name: "{app}\IQToolkit.Data.SQLite.dll"
Type: files; Name: "{app}\MessageLogServer.exe"
Type: files; Name: "{app}\Microsoft.Owin.dll"
Type: files; Name: "{app}\Microsoft.Owin.Host.HttpListener.dll"
Type: files; Name: "{app}\Microsoft.Owin.Hosting.dll"
Type: files; Name: "{app}\Microsoft.Practices.ObjectBuilder2.dll"
Type: files; Name: "{app}\Microsoft.Practices.Unity.dll"
Type: files; Name: "{app}\Owin.dll"
Type: files; Name: "{app}\System.Net.Http.Formatting.dll"
Type: files; Name: "{app}\System.Web.Http.dll"
Type: files; Name: "{app}\System.Web.Http.Owin.dll"
Type: files; Name: "{app}\VirtualRadar.Owin.dll"
Type: files; Name: "{localappdata}\VirtualRadar\AirlineCodes.csv";
Type: files; Name: "{localappdata}\VirtualRadar\AirportCodes.csv";
Type: files; Name: "{localappdata}\VirtualRadar\AircraftTypes.csv";
Type: files; Name: "{localappdata}\VirtualRadar\Countries.dat";
Type: files; Name: "{localappdata}\VirtualRadar\FlightNumbers.csv";

[Icons]
Name: "{group}\Virtual Radar (64-bit)"; Filename: "{app}\VirtualRadar.exe"; WorkingDir: "{app}"

[Run]
; Add permissions on Vista or better for our listener so we don't need to run as an administrator
MinVersion: 5.0,6.0; Filename: "{sys}\netsh.exe"; Parameters: "http add urlacl url=http://*:{code:GetChosenPort}/VirtualRadar/ sddl=D:(A;;GX;;;WD) listen=yes"; Flags: runhidden;
; Optionally add the program to the firewall
MinVersion: 5.0,6.0; Tasks: AddToFirewall; Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall add rule name=""VirtualRadar Server Port {code:GetChosenPort}"" dir=in action=allow protocol=TCP localport={code:GetChosenPort} profile=private"; Flags: runhidden;
OnlyBelowVersion: 1.0,6.0; Tasks: AddToFirewall; Filename: "{sys}\netsh.exe"; Parameters: "firewall add portopening TCP {code:GetChosenPort} ""VirtualRadar Server Port {code:GetChosenPort}"""; Flags: runhidden;


[UninstallRun]
; Remove the permissions that were added for Vista or better
Filename: "{sys}\netsh.exe"; Parameters: "http delete urlacl url=http://*:{code:GetChosenPort}/VirtualRadar/"; MinVersion: 5.0,6.0; Flags: runhidden;
; Remove the program from the firewall
MinVersion: 5.0,6.0; Tasks: AddToFirewall; Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""VirtualRadar Server Port {code:GetChosenPort}"""; Flags: runhidden;
OnlyBelowVersion: 1.0,6.0; Tasks: AddToFirewall; Filename: "{sys}\netsh.exe"; Parameters: "firewall delete portopening TCP {code:GetChosenPort}"; Flags: runhidden;

[Code]
var
  PortPage:     TInputQueryWizardPage;

procedure InitializeWizard;
begin
  PortPage := CreateInputQueryPage(wpSelectDir, 'Server Port', 'Which port should the server listen on?', 'Please enter the port that you would like the server to listen on. You can usually leave this at 80 - choose a value between 1025 and 65535 only if other software is already using port 80.');
  PortPage.Add('Port:', false);
  
  PortPage.Values[0] := GetPreviousData('Port', '80');
end;

function ChosenPort() : string;
var
  port: Integer;
begin
  port := StrToIntDef(PortPage.Values[0], 80);
  if port < 1 then port := 80;
  if port > 65535 then port := 80;

  Result := IntToStr(port);
end;

function GetChosenPort(Param: string) : string;
begin
  Result := ChosenPort();
end;

procedure RegisterPreviousData(PreviousDataKey: Integer);
begin
  SetPreviousData(PreviousDataKey, 'Port', ChosenPort());
end;

function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo,
  MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
var
  msg: String;
begin
  msg := '';
  msg := msg + MemoDirInfo + NewLine;
  msg := msg + NewLine;
  msg := msg + 'Port: ' + NewLine;
  msg := msg + Space + ChosenPort() + NewLine;
  msg := msg + NewLine;
  msg := msg + MemoGroupInfo + NewLine;
  msg := msg + NewLine;
  msg := msg + MemoTasksInfo + NewLine;

  Result := msg;
end;

procedure WriteInstallerConfiguration();
var
  content: TStringList;
  fileName: string;
begin
  content := TStringList.Create();
  content.Add('<?xml version="1.0" encoding="utf-8" ?>');
  content.Add('<InstallerSettings xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">');
  content.Add('  <WebServerPort>' + ChosenPort() + '</WebServerPort>');
  content.Add('</InstallerSettings>');

  fileName := ExpandConstant('{localappdata}');
  fileName := AddBackslash(fileName) + 'VirtualRadar\InstallerConfiguration.xml';
  content.SaveToFile(fileName);
end;

