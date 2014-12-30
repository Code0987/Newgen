#define MyAppId        "{{c3373d77-29c6-4670-8afb-43f0830bc3cf}"
#define MyAppName      "Newgen"
#define MyAppVersion   "13.0.0.0"
#define MyAppURL       "http://NSApps.net/Apps/Newgen"
#define MyAppSize      50

#define IsPortableMode 0
#define IsUpdateSetup  0

#define SourceDir      "D:\Projects\iApps\Newgen\Build\Cache\"
#define FindLicenses   0

#include "D:\Projects\Sandbox\SetupScripts\Installer.Base.iss"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Dirs]
Name: "{app}\Lib"; Flags: deleteafterinstall
Name: "{app}\Widgets"; Flags: deleteafterinstall
Name: "{app}\Cache"; Flags: deleteafterinstall

[Files]
Source: "{#SourceDir}Newgen.exe"; DestDir: "{app}"; Flags: overwritereadonly ignoreversion
Source: "{#SourceDir}Newgen.exe.config"; DestDir: "{app}"; Flags: overwritereadonly ignoreversion
Source: "{#SourceDir}Newgen.Base.dll"; DestDir: "{app}"; Flags: overwritereadonly ignoreversion
Source: "{#SourceDir}Newgen.Base.xml"; DestDir: "{app}"; Flags: overwritereadonly ignoreversion

Source: "{#SourceDir}Cache\BgImage.data"; DestDir: "{app}\Cache"; Flags: overwritereadonly ignoreversion
Source: "{#SourceDir}Lib\*"; DestDir: "{app}\Lib"; Flags: ignoreversion overwritereadonly recursesubdirs

Source: "{#SourceDir}Widgets\Clock\*"; DestDir: "{app}\Widgets\Clock"; Flags: overwritereadonly ignoreversion
Source: "{#SourceDir}Widgets\Computer\*"; DestDir: "{app}\Widgets\Computer"; Flags: overwritereadonly ignoreversion
Source: "{#SourceDir}Widgets\Control Panel\*"; DestDir: "{app}\Widgets\Control Panel"; Flags: overwritereadonly ignoreversion
Source: "{#SourceDir}Widgets\Desktop\*"; DestDir: "{app}\Widgets\Desktop"; Flags: overwritereadonly ignoreversion
Source: "{#SourceDir}Widgets\Gmail\*"; DestDir: "{app}\Widgets\Gmail"; Flags: overwritereadonly ignoreversion
Source: "{#SourceDir}Widgets\Hotmail\*"; DestDir: "{app}\Widgets\Hotmail"; Flags: overwritereadonly ignoreversion
Source: "{#SourceDir}Widgets\Internet\*"; DestDir: "{app}\Widgets\Internet"; Flags: overwritereadonly ignoreversion
Source: "{#SourceDir}Widgets\Store\*"; DestDir: "{app}\Widgets\Store"; Flags: overwritereadonly ignoreversion
Source: "{#SourceDir}Widgets\Weather\*"; DestDir: "{app}\Widgets\Weather"; Flags: overwritereadonly ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppName}.exe"; WorkingDir: "{app}"; IconFilename: "{app}\{#MyAppName}.exe"; Comment: "{#MyAppName}";
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppName}.exe"; WorkingDir: "{app}"; IconFilename: "{app}\{#MyAppName}.exe"; Comment: "{#MyAppName}"; Tasks: desktopicon