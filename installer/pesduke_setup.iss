[Setup]
AppName=PesDuke
AppVersion=1.0
AppPublisher=PesDuke
DefaultDirName={autopf}\PesDuke
DefaultGroupName=PesDuke
OutputDir=C:\Users\fante\Documents\PesDuke\installer\output
OutputBaseFilename=PesDuke-Setup-v1.0
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
SetupIconFile=
UninstallDisplayIcon={app}\PesDuke.exe

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"
Name: "autostart"; Description: "Start PesDuke with Windows"; GroupDescription: "Startup:"

[Files]
Source: "C:\Users\fante\Documents\PesDuke\publish\PesDuke.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\PesDuke"; Filename: "{app}\PesDuke.exe"
Name: "{group}\Uninstall PesDuke"; Filename: "{uninstallexe}"
Name: "{autodesktop}\PesDuke"; Filename: "{app}\PesDuke.exe"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "PesDuke"; ValueData: """{app}\PesDuke.exe"""; Flags: uninsdeletevalue; Tasks: autostart

[Run]
Filename: "{app}\PesDuke.exe"; Description: "Launch PesDuke now"; Flags: nowait postinstall skipifsilent
