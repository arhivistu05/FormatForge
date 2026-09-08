using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FormatForge.App;

internal sealed class PdfCompositionOutcome
{
    public bool Success { get; init; }
    public string OutputPath { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static PdfCompositionOutcome Ok(string outputPath)
    {
        return new PdfCompositionOutcome
        {
            Success = true,
            OutputPath = outputPath,
            Message = "Success"
        };
    }

    public static PdfCompositionOutcome Fail(string message, string outputPath = "")
    {
        return new PdfCompositionOutcome
        {
            Success = false,
            OutputPath = outputPath,
            Message = message
        };
    }
}

internal static class ImagePdfComposer
{
    public static async Task<PdfCompositionOutcome> MergeImagesToPdfAsync(
        IReadOnlyList<string> imagePaths,
        string outputPath,
        IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        if (imagePaths.Count < 2)
        {
            return PdfCompositionOutcome.Fail("Select at least two image files.");
        }

        string[] existingImages = imagePaths
            .Where(path => !string.IsNullOrWhiteSpace(path) && File.Exists(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (existingImages.Length < 2)
        {
            return PdfCompositionOutcome.Fail("At least two selected image files must exist on disk.");
        }

        string? pythonExe = FindPythonExecutable();
        if (pythonExe == null)
        {
            return PdfCompositionOutcome.Fail("PythonRuntime was not found at " + FormatForgePaths.PythonRuntimeDirectory + ".");
        }

        string pythonRoot = FormatForgePaths.PythonDirectory;
        string script = """
import os
import sys

python_root = os.environ.get("FORMATFORGE_PYTHON_ROOT", "")
if python_root and python_root not in sys.path:
    sys.path.insert(0, python_root)

import runtime_bootstrap
runtime_bootstrap.configure()

from engines.image2pdf_engine import merge_images_to_pdf

output_file = sys.argv[1]
image_files = sys.argv[2:]
merge_images_to_pdf(image_files, output_file)
print(output_file)
""";

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? AppContext.BaseDirectory);
        }
        catch (Exception ex)
        {
            return PdfCompositionOutcome.Fail("Output directory could not be created: " + ex.Message, outputPath);
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = pythonExe,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = pythonRoot
        };
        startInfo.ArgumentList.Add("-c");
        startInfo.ArgumentList.Add(script);
        startInfo.ArgumentList.Add(outputPath);
        foreach (string path in existingImages)
        {
            startInfo.ArgumentList.Add(path);
        }

        startInfo.Environment["FORMATFORGE_HOME"] = FormatForgePaths.Root;
        startInfo.Environment["FORMATFORGE_PYTHON_ROOT"] = pythonRoot;
        startInfo.Environment["FORMATFORGE_PYTHON_RUNTIME"] = FormatForgePaths.PythonRuntimeDirectory;
        startInfo.Environment["PYTHONPATH"] = pythonRoot;

        progress?.Report(0.1);

        using Process process = new Process { StartInfo = startInfo };
        try
        {
            process.Start();
            Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            Task<string> stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            string stdout = await stdoutTask;
            string stderr = await stderrTask;

            if (process.ExitCode != 0)
            {
                string message = string.IsNullOrWhiteSpace(stderr) ? stdout.Trim() : stderr.Trim();
                return PdfCompositionOutcome.Fail(string.IsNullOrWhiteSpace(message) ? "Image PDF merge failed." : message, outputPath);
            }

            progress?.Report(1.0);
            return PdfCompositionOutcome.Ok(File.Exists(outputPath) ? outputPath : stdout.Trim());
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            return PdfCompositionOutcome.Fail("PDF creation was cancelled.", outputPath);
        }
        catch (Exception ex)
        {
            TryKill(process);
            return PdfCompositionOutcome.Fail(ex.Message, outputPath);
        }
    }

    private static string? FindPythonExecutable()
    {
        string direct = Path.Combine(FormatForgePaths.PythonRuntimeDirectory, "python.exe");
        if (File.Exists(direct))
        {
            return direct;
        }

        try
        {
            return Directory
                .EnumerateFiles(FormatForgePaths.PythonRuntimeDirectory, "python.exe", SearchOption.AllDirectories)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
        }
    }
}
