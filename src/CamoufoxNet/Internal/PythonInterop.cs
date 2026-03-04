using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CamoufoxNet.Exceptions;

namespace CamoufoxNet.Internal;

internal static class PythonInterop
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    private static string? _cachedBridgePath;

    /// <summary>
    /// Calls the Python bridge to get Camoufox launch options.
    /// </summary>
    public static async Task<string> GetLaunchOptionsJsonAsync(
        CamoufoxOptions options, CancellationToken ct = default)
    {
        var pythonPath = PythonLocator.Find(options.PythonPath);
        var bridgePath = GetBridgeScriptPath();
        var inputJson = SerializeOptions(options);

        var psi = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = $"\"{bridgePath}\"",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi)
            ?? throw new CamoufoxException("Failed to start Python process.");

        await process.StandardInput.WriteAsync(inputJson);
        process.StandardInput.Close();

        var stdout = await process.StandardOutput.ReadToEndAsync(ct);
        var stderr = await process.StandardError.ReadToEndAsync(ct);

        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            if (stderr.Contains("ModuleNotFoundError") || stderr.Contains("No module named"))
                throw new CamoufoxNotInstalledException(
                    $"Python camoufox package not found. Install with: pip install camoufox && camoufox fetch\n\nPython error:\n{stderr}");

            throw new CamoufoxException(
                $"Python bridge exited with code {process.ExitCode}.\nStderr:\n{stderr}");
        }

        if (string.IsNullOrWhiteSpace(stdout))
            throw new CamoufoxException(
                $"Python bridge returned empty output.\nStderr:\n{stderr}");

        return stdout;
    }

    /// <summary>
    /// Serializes CamoufoxOptions to the JSON format expected by launch_bridge.py.
    /// </summary>
    internal static string SerializeOptions(CamoufoxOptions options)
    {
        var dict = new Dictionary<string, object?>();

        if (options.TargetOs is { Count: > 0 })
            dict["os"] = options.TargetOs;

        dict["headless"] = options.Headless;

        if (options.Humanize)
        {
            if (options.HumanizeMaxDurationSeconds.HasValue)
                dict["humanize"] = options.HumanizeMaxDurationSeconds.Value;
            else
                dict["humanize"] = true;
        }

        if (options.GeoIpAutoDetect)
            dict["geoip"] = true;
        else if (!string.IsNullOrEmpty(options.GeoIp))
            dict["geoip"] = options.GeoIp;

        if (options.Locales is { Count: > 0 })
            dict["locale"] = options.Locales;

        if (options.BlockImages) dict["block_images"] = true;
        if (options.BlockWebRtc) dict["block_webrtc"] = true;
        if (options.BlockWebGl) dict["block_webgl"] = true;
        if (options.DisableCoop) dict["disable_coop"] = true;

        if (options.Screen is not null)
        {
            var screen = new Dictionary<string, object?>();
            if (options.Screen.MinWidth.HasValue) screen["min_width"] = options.Screen.MinWidth;
            if (options.Screen.MaxWidth.HasValue) screen["max_width"] = options.Screen.MaxWidth;
            if (options.Screen.MinHeight.HasValue) screen["min_height"] = options.Screen.MinHeight;
            if (options.Screen.MaxHeight.HasValue) screen["max_height"] = options.Screen.MaxHeight;
            dict["screen"] = screen;
        }

        if (options.Proxy is not null)
        {
            var proxy = new Dictionary<string, object?> { ["server"] = options.Proxy.Server };
            if (!string.IsNullOrEmpty(options.Proxy.Username)) proxy["username"] = options.Proxy.Username;
            if (!string.IsNullOrEmpty(options.Proxy.Password)) proxy["password"] = options.Proxy.Password;
            if (!string.IsNullOrEmpty(options.Proxy.Bypass)) proxy["bypass"] = options.Proxy.Bypass;
            dict["proxy"] = proxy;
        }

        if (options.Fonts is { Count: > 0 }) dict["fonts"] = options.Fonts;
        if (options.Addons is { Count: > 0 }) dict["addons"] = options.Addons;
        if (options.ExcludeAddons is { Count: > 0 }) dict["exclude_addons"] = options.ExcludeAddons;

        if (options.Window.HasValue)
            dict["window"] = new[] { options.Window.Value.Width, options.Window.Value.Height };

        if (options.WebGlConfig.HasValue)
            dict["webgl_config"] = new[] { options.WebGlConfig.Value.Vendor, options.WebGlConfig.Value.Renderer };

        if (options.Config is { Count: > 0 }) dict["config"] = options.Config;
        if (options.FirefoxUserPrefs is { Count: > 0 }) dict["firefox_user_prefs"] = options.FirefoxUserPrefs;
        if (!string.IsNullOrEmpty(options.ExecutablePath)) dict["executable_path"] = options.ExecutablePath;
        if (options.EnableCache) dict["enable_cache"] = true;
        if (options.Args is { Count: > 0 }) dict["args"] = options.Args;
        if (options.Env is { Count: > 0 }) dict["env"] = options.Env;
        if (options.MainWorldEval) dict["main_world_eval"] = true;
        if (options.FirefoxVersion.HasValue) dict["ff_version"] = options.FirefoxVersion.Value;

        return JsonSerializer.Serialize(dict, SerializerOptions);
    }

    /// <summary>
    /// Extracts the embedded launch_bridge.py to a temp directory and returns its path.
    /// The file is cached for the lifetime of the process.
    /// </summary>
    internal static string GetBridgeScriptPath()
    {
        if (_cachedBridgePath is not null && File.Exists(_cachedBridgePath))
            return _cachedBridgePath;

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("launch_bridge.py"))
            ?? throw new CamoufoxException("Embedded launch_bridge.py resource not found.");

        var tempDir = Path.Combine(Path.GetTempPath(), "CamoufoxNet");
        Directory.CreateDirectory(tempDir);
        var scriptPath = Path.Combine(tempDir, "launch_bridge.py");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var fs = File.Create(scriptPath);
        stream.CopyTo(fs);

        _cachedBridgePath = scriptPath;
        return scriptPath;
    }
}
