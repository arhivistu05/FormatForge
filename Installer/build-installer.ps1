param(
    [string]$Configuration = "Release",
    [switch]$FrameworkDependent,
    [string]$InnoCompiler = ""
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "== $Message ==" -ForegroundColor Cyan
}

function Copy-DirectoryFiltered {
    param(
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][string]$Destination,
        [string[]]$ExcludedDirectoryNames = @()
    )

    if (-not (Test-Path -Path $Source -PathType Container)) {
        return
    }

    New-Item -ItemType Directory -Force -Path $Destination | Out-Null

    Get-ChildItem -Path $Source -Force | ForEach-Object {
        if ($_.PSIsContainer -and ($ExcludedDirectoryNames -contains $_.Name)) {
            return
        }

        $target = Join-Path $Destination $_.Name
        if ($_.PSIsContainer) {
            Copy-DirectoryFiltered -Source $_.FullName -Destination $target -ExcludedDirectoryNames $ExcludedDirectoryNames
        }
        else {
            Copy-Item -Path $_.FullName -Destination $target -Force
        }
    }
}

function Resolve-InnoCompiler {
    param([string]$ExplicitPath)

    $candidates = @()
    if (-not [string]::IsNullOrWhiteSpace($ExplicitPath)) {
        $candidates += $ExplicitPath
    }

    $command = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($command) {
        $candidates += $command.Source
    }

    $programFilesX86 = [Environment]::GetEnvironmentVariable("ProgramFiles(x86)")
    if (-not [string]::IsNullOrWhiteSpace($programFilesX86)) {
        $candidates += (Join-Path $programFilesX86 "Inno Setup 6\ISCC.exe")
    }

    if (-not [string]::IsNullOrWhiteSpace($env:ProgramFiles)) {
        $candidates += (Join-Path $env:ProgramFiles "Inno Setup 6\ISCC.exe")
    }

    foreach ($candidate in $candidates) {
        if (-not [string]::IsNullOrWhiteSpace($candidate) -and (Test-Path -Path $candidate -PathType Leaf)) {
            return $candidate
        }
    }

    return $null
}

$installerDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = Split-Path -Parent $installerDir
$project = Join-Path $root "App\FormatForge.App\FormatForge.App.csproj"
$appInfo = Join-Path $root "App\FormatForge.App\AppInfo.cs"
$issPath = Join-Path $installerDir "FormatForge.iss"
$packageRoot = Join-Path $installerDir "Build\Package"
$publishDir = Join-Path $packageRoot "App\FormatForge.App"
$dllSource = Join-Path $root "DLL"
$pythonSource = Join-Path $root "Python"
$ffmpegBinSource = Join-Path $root "FFmpeg\bin"

if (-not (Test-Path -Path $project -PathType Leaf)) {
    throw "Application project was not found: $project"
}

$version = "1.1.0"
if (Test-Path -Path $appInfo -PathType Leaf) {
    $appInfoText = [System.IO.File]::ReadAllText($appInfo)
    if ($appInfoText -match 'Version\s*=\s*"([^"]+)"') {
        $version = $Matches[1]
    }
}

if (Test-Path -Path $issPath -PathType Leaf) {
    $issText = [System.IO.File]::ReadAllText($issPath)
    $issText = [regex]::Replace($issText, '#define MyAppVersion "[^"]+"', '#define MyAppVersion "' + $version + '"', 1)
    [System.IO.File]::WriteAllText($issPath, $issText, [System.Text.Encoding]::UTF8)
}

Write-Step "Cleaning installer build folder"
if (Test-Path -Path $packageRoot) {
    Remove-Item -Path $packageRoot -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null

Write-Step "Publishing FormatForge.App"
$selfContained = -not $FrameworkDependent.IsPresent
$publishArgs = @(
    "publish",
    $project,
    "-c", $Configuration,
    "-r", "win-x64",
    "--self-contained", $selfContained.ToString().ToLowerInvariant(),
    "-p:PublishSingleFile=false",
    "-p:Platform=x64",
    "-o", $publishDir
)

& dotnet @publishArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed."
}

Write-Step "Copying native DLL folder"
$dllDestination = Join-Path $packageRoot "DLL"
New-Item -ItemType Directory -Force -Path $dllDestination | Out-Null
if (Test-Path -Path $dllSource -PathType Container) {
    Get-ChildItem -Path $dllSource -Filter "*.dll" -File | ForEach-Object {
        Copy-Item -Path $_.FullName -Destination (Join-Path $dllDestination $_.Name) -Force
    }

    $pythonRuntimeSource = Join-Path $dllSource "PythonRuntime"
    if (Test-Path -Path $pythonRuntimeSource -PathType Container) {
        Copy-DirectoryFiltered -Source $pythonRuntimeSource -Destination (Join-Path $dllDestination "PythonRuntime") -ExcludedDirectoryNames @("__pycache__")
    }
    else {
        Write-Warning "PythonRuntime was not found at $pythonRuntimeSource. Python/PDF conversions will require it."
    }
}
else {
    throw "DLL source folder was not found: $dllSource"
}

Write-Step "Copying Python scripts"
if (Test-Path -Path $pythonSource -PathType Container) {
    Copy-DirectoryFiltered -Source $pythonSource -Destination (Join-Path $packageRoot "Python") -ExcludedDirectoryNames @("venv", "__pycache__", ".pytest_cache")
}
else {
    Write-Warning "Python scripts folder was not found: $pythonSource"
}

Write-Step "Copying FFmpeg runtime if available"
if (Test-Path -Path $ffmpegBinSource -PathType Container) {
    Copy-DirectoryFiltered -Source $ffmpegBinSource -Destination (Join-Path $packageRoot "FFmpeg\bin") -ExcludedDirectoryNames @()
}
else {
    Write-Warning "FFmpeg bin folder was not found: $ffmpegBinSource. Audio/video conversion will require FFmpeg from PATH."
}

Write-Step "Copying release notes"
$readmeSource = Join-Path $root "README.md"
if (Test-Path -Path $readmeSource -PathType Leaf) {
    Copy-Item -Path $readmeSource -Destination (Join-Path $packageRoot "README.md") -Force
}
Copy-Item -Path (Join-Path $installerDir "TERMS.txt") -Destination (Join-Path $packageRoot "TERMS.txt") -Force

$requiredDlls = @(
    "converter_core.dll",
    "image_converter.dll",
    "audio_converter.dll",
    "video_converter.dll",
    "PythonConverter.dll"
)

foreach ($dll in $requiredDlls) {
    $dllPath = Join-Path $dllDestination $dll
    if (-not (Test-Path -Path $dllPath -PathType Leaf)) {
        throw "Required runtime DLL is missing from package: $dllPath"
    }
}

$exePath = Join-Path $publishDir "FormatForge.App.exe"
if (-not (Test-Path -Path $exePath -PathType Leaf)) {
    throw "Published app executable was not found: $exePath"
}

$manifest = @"
FormatForge installer package
Version: $version
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

Entry point:
App\FormatForge.App\FormatForge.App.exe

Runtime folders:
DLL\
DLL\PythonRuntime\
Python\
FFmpeg\bin\
"@
[System.IO.File]::WriteAllText((Join-Path $packageRoot "INSTALLER_CONTENTS.txt"), $manifest, [System.Text.Encoding]::UTF8)

Write-Step "Compiling installer"
$compiler = Resolve-InnoCompiler -ExplicitPath $InnoCompiler
if ($null -eq $compiler) {
    Write-Warning "Inno Setup 6 compiler was not found."
    Write-Host "Install Inno Setup 6, then run this script again:"
    $commandLine = 'powershell.exe -NoProfile -ExecutionPolicy Bypass -File "' + $MyInvocation.MyCommand.Path + '"'
    Write-Host $commandLine
    Write-Host ""
    Write-Host "The installer package folder was still prepared here:"
    Write-Host $packageRoot
    exit 2
}

Push-Location $installerDir
try {
    & $compiler $issPath
    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup compilation failed."
    }
}
finally {
    Pop-Location
}

$installerPath = Join-Path $installerDir ("Output\FormatForgeSetup-{0}-x64.exe" -f $version)
Write-Step "Done"
if (Test-Path -Path $installerPath -PathType Leaf) {
    Write-Host "Installer created:"
    Write-Host $installerPath
}
else {
    Write-Host "Installer output folder:"
    Write-Host (Join-Path $installerDir "Output")
}