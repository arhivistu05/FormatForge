# FormatForge

FormatForge is a Windows desktop file conversion app for images, audio, video, and documents. It combines a C# Windows Forms interface with native C++ converter libraries and Python-powered PDF/document tooling.

Created by `./F4N3`.

## Features

- Queue files and folders for conversion.
- Filter files by type and tags.
- Convert images, audio, video, and supported document inputs.
- Choose output formats per file type.
- Preview images and embedded audio album art.
- Combine multiple images into a single PDF.
- Light and dark themes.
- Runtime checks for required Windows dependencies.
- Startup update notification through GitHub Releases.

## Project Layout

```text
App/FormatForge.App/      C# Windows Forms application
Native/Core/              converter_core C++ project
Native/Image/             image converter C++ sources and import library
Native/Audio/             audio converter C++ sources and import library
Native/Video/             video converter C++ sources and import library
Native/PythonConverter/   PythonConverter C++ bridge project
Python/                   Python conversion scripts and engines
Resources/                shared project resources
DLL/                      small runtime DLLs used by the app
```

Large external runtimes are intentionally not committed:

- `DLL/PythonRuntime/`
- `FFmpeg/`
- Python virtual environments
- Visual Studio build output
- backup/staging folders

## Requirements

### Build Requirements

- Windows 10 or newer
- Visual Studio 2026 or newer with:
  - .NET desktop development
  - Desktop development with C++
  - MSVC x64 tools
- .NET SDK targeting `net10.0-windows`

### Runtime Dependencies

FormatForge expects these components to be available on the user's Windows machine:

- **FFmpeg** for audio/video conversion workflows.
- **Microsoft Office** for Office document conversion workflows.
- **PythonRuntime** in `DLL/PythonRuntime/` for Python-backed PDF/document tools.

If FFmpeg or Microsoft Office are not found, the app shows a startup warning. The application can still open, but conversions that depend on the missing component may fail.

## GitHub Release Update Check

The startup release check is configured in:

```text
App/FormatForge.App/AppInfo.cs
```

Current values:

```csharp
public const string LatestReleaseApiUrl = "https://api.github.com/repos/arhivistu05/FormatForge/releases/latest";
public const string ReleasesPageUrl = "https://github.com/arhivistu05/FormatForge/releases/latest";
```

When a GitHub release has a newer tag than `AppInfo.Version`, FormatForge shows a startup dialog with a button to open the latest release page.

## Build

From PowerShell:

```powershell
dotnet build .\App\FormatForge.App\FormatForge.App.csproj -c Release -p:Platform=x64
```

The executable is generated under:

```text
App/FormatForge.App/bin/x64/Release/net10.0-windows/FormatForge.App.exe
```

## Notes for Releases

Before publishing a release, include the required runtime components next to the app:

- `converter_core.dll`
- `image_converter.dll`
- `audio_converter.dll`
- `video_converter.dll`
- `PythonConverter.dll`
- `DLL/PythonRuntime/`
- FFmpeg, either bundled according to its license or installed separately and available in PATH

Microsoft Office is not bundled. It must be installed separately on Windows for Office conversion support.

## License

License information has not been added yet.
