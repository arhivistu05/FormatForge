#define MyAppName "FormatForge"
#define MyAppVersion "1.1.0"
#define MyAppPublisher "./F4N3"
#define MyAppExeName "FormatForge.App.exe"
#define MyAppPackageRoot "Build\Package"
#define MyAppExePath "App\FormatForge.App\FormatForge.App.exe"

[Setup]
AppId={{B8BC6333-44D4-4C8F-BB3E-B433AB399D0B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\FormatForge
DefaultGroupName=FormatForge
DisableProgramGroupPage=yes
LicenseFile=TERMS.txt
OutputDir=Output
OutputBaseFilename=FormatForgeSetup-{#MyAppVersion}-x64
SetupIconFile=..\Resources\Icons\formatforge_icon.ico
UninstallDisplayIcon={app}\App\FormatForge.App\Assets\formatforge.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
WizardResizable=yes
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
CloseApplications=yes
RestartIfNeededByRun=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Shortcuts:"; Flags: unchecked
Name: "startmenuicon"; Description: "Create a Start Menu shortcut"; GroupDescription: "Shortcuts:"

[Files]
Source: "{#MyAppPackageRoot}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\FormatForge"; Filename: "{app}\{#MyAppExePath}"; WorkingDir: "{app}"; IconFilename: "{app}\App\FormatForge.App\Assets\formatforge.ico"; Tasks: startmenuicon
Name: "{autodesktop}\FormatForge"; Filename: "{app}\{#MyAppExePath}"; WorkingDir: "{app}"; IconFilename: "{app}\App\FormatForge.App\Assets\formatforge.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExePath}"; Description: "Run FormatForge"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent