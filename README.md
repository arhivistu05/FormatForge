# 🔥 FormatForge

**Offline-first file conversion for Windows.**

FormatForge is a Windows desktop application for converting, organizing,
and managing files from one place. It combines a C# Windows Forms
interface with native C++ converter libraries and Python-powered
PDF/document tooling.

Created by `./F4N3`.

## ✨ Features

-   🖼️ Convert image files between supported formats.
-   🎵 Convert audio files with FFmpeg-backed workflows.
-   🎬 Convert video files with FFmpeg-backed workflows.
-   📄 Convert supported document formats and generate PDF output.
-   📁 Add individual files or entire folders to a conversion queue.
-   🏷️ Filter and organize queued files by type, status, and tags.
-   👁️ Preview images and embedded audio album art.
-   🧩 Combine multiple images into a single PDF.
-   🌙 Use light and dark themes.
-   🔍 Check for required runtime dependencies at startup.
-   🚀 Notify the user when a newer GitHub release is available.

## 📁 Project Structure

``` text
FormatForge/
├── App/
│   └── FormatForge.App/        # C# Windows Forms application
├── Native/
│   ├── Core/                   # converter_core native API
│   ├── Image/                  # image_converter native library
│   ├── Audio/                  # audio_converter native library
│   ├── Video/                  # video_converter native library
│   └── PythonConverter/        # native bridge between C++ and Python
├── Python/                     # Python conversion scripts
├── Resources/                  # icons, themes, and shared assets
├── DLL/                        # runtime DLLs used by the app
└── FFmpeg/                     # optional local FFmpeg runtime
```

Large runtime folders, generated build output, staging folders, logs,
and backups are intentionally excluded from source control.

## ⚙️ Architecture

``` text
FormatForge.App
     |
     v
converter_core.dll
     |
     +--> image_converter.dll
     |
     +--> audio_converter.dll --> FFmpeg
     |
     +--> video_converter.dll --> FFmpeg
     |
     +--> PythonConverter.dll --> PythonRuntime --> PDF/document tools
```

The user interface stays separate from the conversion engines. The
native layer exposes the main conversion API, FFmpeg handles audio/video
workflows, and Python is used for PDF and document-related
functionality.

## 🛠️ Requirements

### 🧰 Build Requirements

-   Windows 10 or newer, x64
-   Visual Studio with:
    -   .NET desktop development workload
    -   Desktop development with C++ workload
    -   MSVC x64 build tools
-   .NET SDK compatible with `net10.0-windows`

### 📦 Runtime Dependencies

  -----------------------------------------------------------------------
  Component               Used for                Notes
  ----------------------- ----------------------- -----------------------
  FFmpeg                  Audio and video         Required for
                          conversion              audio/video workflows.
                                                  It can be bundled
                                                  locally or installed
                                                  separately and
                                                  available through
                                                  `PATH`.

  Microsoft Office        Office document         Required only for
                          conversion              workflows that depend
                                                  on Office formats.
                                                  Microsoft Office is not
                                                  bundled with
                                                  FormatForge.

  PythonRuntime           Python-backed           Used by
                          PDF/document tools      `PythonConverter.dll`
                                                  and the scripts in
                                                  `Python/`.
  -----------------------------------------------------------------------

FormatForge can open even if optional dependencies are missing, but
conversions that rely on missing components may fail. The application
reports missing FFmpeg or Microsoft Office installations at startup.

## 🔨 Build

From the repository root:

``` powershell
dotnet build .\App\FormatForge.App\FormatForge.App.csproj -c Release -p:Platform=x64
```

Expected output:

``` text
App/FormatForge.App/bin/x64/Release/net10.0-windows/FormatForge.App.exe
```

## 🚀 GitHub Release Update Check

Startup update checking is configured in:

``` text
App/FormatForge.App/AppInfo.cs
```

Update these constants when moving the project to another repository or
release page:

``` csharp
public const string LatestReleaseApiUrl =
    "https://api.github.com/repos/arhivistu05/FormatForge/releases/latest";

public const string ReleasesPageUrl =
    "https://github.com/arhivistu05/FormatForge/releases/latest";
```

When a GitHub release tag is newer than `AppInfo.Version`, FormatForge
displays a startup dialog with a button that opens the latest release
page.

## 📦 Release Package

A complete release package should include:

``` text
FormatForge.App.exe
converter_core.dll
image_converter.dll
audio_converter.dll
video_converter.dll
PythonConverter.dll
DLL/
PythonRuntime/
```

If FFmpeg is bundled with the release, include it according to its
license. Otherwise, users must install FFmpeg separately and make it
available through `PATH`.

Microsoft Office is not redistributed with FormatForge. Users must
install it separately if they need Office-based conversion support.

## 🧱 Technology Stack

  Layer                  Technology
  ---------------------- --------------------
  Desktop UI             C# / Windows Forms
  Native core            C++
  Image conversion       Native C++
  Audio conversion       C++ + FFmpeg
  Video conversion       C++ + FFmpeg
  Document/PDF tooling   Python
  Native/Python bridge   C++
  Platform               Windows x64

## 🗺️ Roadmap

-   [x] Windows Forms interface
-   [x] Native converter core
-   [x] Image conversion library
-   [x] FFmpeg-backed audio conversion
-   [x] FFmpeg-backed video conversion
-   [x] Python conversion scripts
-   [x] Native Python bridge
-   [x] Light and dark themes
-   [x] GitHub release update checking
-   [ ] Installer
-   [ ] Automated release builds
-   [ ] More conversion formats

## 📜 License

License information has not been added yet.

## 👤 Author

FormatForge is created and maintained by `./F4N3`.
