::: {align="center"}
# 🔥 FormatForge

### Offline-first file conversion for Windows

**Images • Audio • Video • Documents • PDF**

FormatForge is a Windows desktop file conversion app that combines a
**C# Windows Forms interface**, **native C++ converter libraries**, and
**Python-powered PDF/document tooling**.

**Created by `./F4N3`**
:::

------------------------------------------------------------------------

## ✨ Features

-   🖼️ Convert images between supported formats.
-   🎵 Convert audio using FFmpeg.
-   🎬 Convert video using FFmpeg.
-   📄 Convert supported documents and generate PDFs.
-   📁 Queue individual files or entire folders.
-   🏷️ Filter and organize files by type and tags.
-   👁️ Preview images and embedded audio album art.
-   🧩 Combine multiple images into a single PDF.
-   🌙 Light and dark themes.
-   🔍 Runtime checks for required Windows dependencies.
-   🚀 Startup update notifications through GitHub Releases.

------------------------------------------------------------------------

## 🖥️ Interface

```{=html}
<p align="center">
```
`<img src="Resources/GitHub/FormatForge-Main.png" alt="FormatForge main interface" width="900">`{=html}
```{=html}
</p>
```
> Add the current application screenshot as
> `Resources/GitHub/FormatForge-Main.png`.

------------------------------------------------------------------------

## ⚙️ Architecture

``` text
┌─────────────────────────────────────┐
│           FormatForge.App           │
│          C# Windows Forms           │
└──────────────────┬──────────────────┘
                   │
                   ▼
┌─────────────────────────────────────┐
│          converter_core.dll         │
│            Native C++ API           │
└────────┬──────────┬──────────┬──────┘
         │          │          │
         ▼          ▼          ▼
 image_converter  audio_converter  video_converter
      .dll             .dll             .dll
                         │                │
                         └───────┬────────┘
                                 ▼
                               FFmpeg

                   ┌─────────────────────┐
                   │ PythonConverter.dll │
                   │    C++ ↔ Python     │
                   └──────────┬──────────┘
                              ▼
                       PythonRuntime
                              │
                              ▼
                      PDF / Documents
```

FormatForge keeps the user interface separate from its conversion
engines. Native C++ libraries provide the conversion layer, while Python
provides additional PDF and document functionality.

------------------------------------------------------------------------

## 📂 Project Layout

``` text
FormatForge/
│
├── App/
│   └── FormatForge.App/        # C# Windows Forms application
│
├── Native/
│   ├── Core/                   # converter_core C++ project
│   ├── Image/                  # Image converter C++ sources/import library
│   ├── Audio/                  # Audio converter C++ sources/import library
│   ├── Video/                  # Video converter C++ sources/import library
│   └── PythonConverter/        # Native C++ ↔ Python bridge
│
├── Python/                     # Python conversion scripts and engines
├── Resources/                  # Icons, themes and shared resources
├── DLL/                        # Small runtime DLLs used by the app
└── FFmpeg/                     # Optional local FFmpeg runtime
```

Large external runtimes and generated files are intentionally excluded:

``` text
DLL/PythonRuntime/
FFmpeg/
Python virtual environments
Visual Studio build output
backup/staging folders
```

------------------------------------------------------------------------

## 🔄 Conversion Pipeline

``` text
User
 │
 ▼
FormatForge UI
 │
 ▼
Conversion Request
 │
 ├── Image ───────► image_converter.dll
 │
 ├── Audio ───────► audio_converter.dll ─────► FFmpeg
 │
 ├── Video ───────► video_converter.dll ─────► FFmpeg
 │
 └── Document ────► PythonConverter.dll
                           │
                           ▼
                     PythonRuntime
                           │
                           ▼
                    PDF / Documents
```

------------------------------------------------------------------------

## 🛠️ Build Requirements

FormatForge currently targets **Windows x64**.

-   Windows 10 or newer
-   Visual Studio 2026 or newer
-   .NET SDK targeting `net10.0-windows`
-   **.NET desktop development** workload
-   **Desktop development with C++** workload
-   MSVC x64 build tools

------------------------------------------------------------------------

## 📦 Runtime Dependencies

  -----------------------------------------------------------------------
  Component               Used for                Required when
  ----------------------- ----------------------- -----------------------
  **FFmpeg**              Audio and video         Using audio/video
                          conversion              workflows

  **Microsoft Office**    Office document         Converting Office
                          conversion              formats

  **PythonRuntime**       Python-backed           Using Python conversion
                          PDF/document tools      engines
  -----------------------------------------------------------------------

FormatForge checks required runtime components during startup. If FFmpeg
or Microsoft Office is not found, the application can still open, but
conversions that depend on the missing component may fail.

------------------------------------------------------------------------

## 🔨 Build

From PowerShell:

``` powershell
dotnet build .\App\FormatForge.App\FormatForge.App.csproj -c Release -p:Platform=x64
```

Output:

``` text
App/FormatForge.App/bin/x64/Release/net10.0-windows/FormatForge.App.exe
```

------------------------------------------------------------------------

## 🚀 GitHub Release Update Check

The startup release check is configured in:

``` text
App/FormatForge.App/AppInfo.cs
```

``` csharp
public const string LatestReleaseApiUrl =
    "https://api.github.com/repos/arhivistu05/FormatForge/releases/latest";

public const string ReleasesPageUrl =
    "https://github.com/arhivistu05/FormatForge/releases/latest";
```

When a GitHub release has a newer tag than `AppInfo.Version`,
FormatForge shows a startup dialog with a button to open the latest
release page.

------------------------------------------------------------------------

## 📦 Release Contents

A complete release should include the required runtime components:

``` text
FormatForge.App.exe
converter_core.dll
image_converter.dll
audio_converter.dll
video_converter.dll
PythonConverter.dll

DLL/
└── PythonRuntime/

FFmpeg/     # if bundled according to its license
```

FFmpeg may instead be installed separately and available through `PATH`.

Microsoft Office is **not bundled**. It must be installed separately
when Office-based conversion support is required.

------------------------------------------------------------------------

## 🧱 Technology Stack

  Layer                  Technology
  ---------------------- --------------------
  Desktop UI             C# / Windows Forms
  Native core            C++
  Image conversion       Native C++
  Audio conversion       C++ + FFmpeg
  Video conversion       C++ + FFmpeg
  Document/PDF tools     Python
  Native/Python bridge   C++
  Platform               Windows x64

------------------------------------------------------------------------

## 🗺️ Roadmap

-   [x] Windows Forms interface
-   [x] Native image conversion
-   [x] FFmpeg audio conversion
-   [x] FFmpeg video conversion
-   [x] Python conversion engines
-   [x] Native/Python bridge
-   [x] Light and dark themes
-   [x] GitHub release update checking
-   [ ] Installer
-   [ ] Automated release builds
-   [ ] Additional conversion formats

------------------------------------------------------------------------

## 📜 License

License information has not been added yet.

------------------------------------------------------------------------

::: {align="center"}
### 🔥 FormatForge

**Forge your files into the format you need.**

Created by `./F4N3`
:::
