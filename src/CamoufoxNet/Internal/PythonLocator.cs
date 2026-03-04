using CamoufoxNet.Exceptions;

namespace CamoufoxNet.Internal;

internal static class PythonLocator
{
    private static readonly string[] PythonCandidates = OperatingSystem.IsWindows()
        ? ["python", "python3", "py"]
        : ["python3", "python"];

    /// <summary>
    /// Finds a working Python 3.10+ executable.
    /// </summary>
    public static string Find(string? customPath = null)
    {
        if (!string.IsNullOrEmpty(customPath))
        {
            if (File.Exists(customPath))
                return customPath;

            throw new PythonNotFoundException($"Specified Python path not found: {customPath}");
        }

        foreach (var candidate in PythonCandidates)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = candidate,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var process = System.Diagnostics.Process.Start(psi);
                if (process is null) continue;

                process.WaitForExit(5000);
                if (process.ExitCode == 0)
                    return candidate;
            }
            catch
            {
                // candidate not found, try next
            }
        }

        throw new PythonNotFoundException();
    }
}
