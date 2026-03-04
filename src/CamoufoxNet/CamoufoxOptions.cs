namespace CamoufoxNet;

public sealed record CamoufoxOptions
{
    /// <summary>Target OS(es) for fingerprint generation (e.g., "windows", "macos", "linux").</summary>
    public IReadOnlyList<string>? TargetOs { get; init; }

    /// <summary>Run in headless mode.</summary>
    public bool Headless { get; init; }

    /// <summary>Proxy configuration.</summary>
    public ProxyOptions? Proxy { get; init; }

    /// <summary>Block image loading.</summary>
    public bool BlockImages { get; init; }

    /// <summary>Block WebRTC to prevent IP leaks.</summary>
    public bool BlockWebRtc { get; init; }

    /// <summary>Block WebGL.</summary>
    public bool BlockWebGl { get; init; }

    /// <summary>Disable COOP headers.</summary>
    public bool DisableCoop { get; init; }

    /// <summary>Custom WebGL vendor/renderer pair.</summary>
    public (string Vendor, string Renderer)? WebGlConfig { get; init; }

    /// <summary>GeoIP country code for locale/timezone spoofing.</summary>
    public string? GeoIp { get; init; }

    /// <summary>Auto-detect GeoIP from the proxy's IP address.</summary>
    public bool GeoIpAutoDetect { get; init; }

    /// <summary>Enable humanized cursor movements. Set HumanizeMaxDurationSeconds for custom timing.</summary>
    public bool Humanize { get; init; }

    /// <summary>Max duration in seconds for humanized cursor movements.</summary>
    public double? HumanizeMaxDurationSeconds { get; init; }

    /// <summary>Browser locale(s).</summary>
    public IReadOnlyList<string>? Locales { get; init; }

    /// <summary>Paths to Firefox addon XPI files to install.</summary>
    public IReadOnlyList<string>? Addons { get; init; }

    /// <summary>Default addon names to exclude.</summary>
    public IReadOnlyList<string>? ExcludeAddons { get; init; }

    /// <summary>Custom font families to use.</summary>
    public IReadOnlyList<string>? Fonts { get; init; }

    /// <summary>Browser window size (width, height).</summary>
    public (int Width, int Height)? Window { get; init; }

    /// <summary>Screen size constraints for fingerprint generation.</summary>
    public ScreenConstraints? Screen { get; init; }

    /// <summary>Target Firefox version for fingerprint.</summary>
    public int? FirefoxVersion { get; init; }

    /// <summary>Enable main world script evaluation.</summary>
    public bool MainWorldEval { get; init; }

    /// <summary>Custom path to the Camoufox browser executable.</summary>
    public string? ExecutablePath { get; init; }

    /// <summary>Custom Firefox user preferences.</summary>
    public Dictionary<string, object>? FirefoxUserPrefs { get; init; }

    /// <summary>Enable browser cache.</summary>
    public bool EnableCache { get; init; }

    /// <summary>Extra command-line arguments for the browser.</summary>
    public IReadOnlyList<string>? Args { get; init; }

    /// <summary>Extra environment variables for the browser process.</summary>
    public Dictionary<string, string>? Env { get; init; }

    /// <summary>Extra fingerprint configuration overrides.</summary>
    public Dictionary<string, object?>? Config { get; init; }

    /// <summary>Enable debug output from the Python bridge.</summary>
    public bool Debug { get; init; }

    /// <summary>Path for persistent browser context (user data directory).</summary>
    public string? PersistentContextPath { get; init; }

    /// <summary>Playwright timeout in milliseconds.</summary>
    public float? TimeoutMs { get; init; }

    /// <summary>Playwright slow-mo in milliseconds.</summary>
    public float? SlowMo { get; init; }

    /// <summary>Custom path to the Python executable.</summary>
    public string? PythonPath { get; init; }
}

public sealed record ProxyOptions
{
    /// <summary>Proxy server URL (e.g., "http://host:port" or "socks5://host:port").</summary>
    public required string Server { get; init; }

    /// <summary>Proxy authentication username.</summary>
    public string? Username { get; init; }

    /// <summary>Proxy authentication password.</summary>
    public string? Password { get; init; }

    /// <summary>Comma-separated list of hosts to bypass the proxy.</summary>
    public string? Bypass { get; init; }
}

public sealed record ScreenConstraints(
    int? MinWidth = null,
    int? MaxWidth = null,
    int? MinHeight = null,
    int? MaxHeight = null);
