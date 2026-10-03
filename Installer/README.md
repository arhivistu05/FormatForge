# FormatForge Installer

This folder contains the installer definition for FormatForge.

Files:

- FormatForge.iss: Inno Setup installer script.
- TERMS.txt: terms and conditions shown during setup.
- build-installer.ps1: builds/publishes the app, prepares the package, and compiles the setup executable when Inno Setup 6 is installed.

Build command:

    powershell.exe -NoProfile -ExecutionPolicy Bypass -File "F:\FormatForge\Installer\build-installer.ps1"

Output folder:

    F:\FormatForge\Installer\Output\

By default the script publishes a self-contained Windows x64 app, so users do not need to install the .NET runtime separately. For a smaller installer that requires the .NET Desktop Runtime on the user's machine, run:

    powershell.exe -NoProfile -ExecutionPolicy Bypass -File "F:\FormatForge\Installer\build-installer.ps1" -FrameworkDependent

Installer options:

- Terms and conditions page.
- Optional Desktop shortcut.
- Optional Start Menu shortcut.
- Optional Run FormatForge checkbox after installation.

Installed layout:

    FormatForge\
    App\FormatForge.App\
    DLL\
    DLL\PythonRuntime\
    Python\
    FFmpeg\bin\