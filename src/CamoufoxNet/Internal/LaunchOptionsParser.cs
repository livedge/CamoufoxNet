using System.Text.Json;
using Microsoft.Playwright;

namespace CamoufoxNet.Internal;

internal static class LaunchOptionsParser
{
    /// <summary>
    /// Parses the JSON output from the Python bridge into BrowserTypeLaunchOptions.
    /// </summary>
    public static BrowserTypeLaunchOptions Parse(string json, CamoufoxOptions options)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var launchOptions = new BrowserTypeLaunchOptions
        {
            ExecutablePath = root.GetProperty("executable_path").GetString(),
            Headless = root.TryGetProperty("headless", out var h) && h.GetBoolean(),
            Args = ParseStringArray(root, "args"),
            Env = ParseStringDictionary(root, "env"),
            FirefoxUserPrefs = ParseMixedDictionary(root, "firefox_user_prefs"),
        };

        if (root.TryGetProperty("proxy", out var proxyEl) && proxyEl.ValueKind == JsonValueKind.Object)
        {
            launchOptions.Proxy = new Proxy
            {
                Server = proxyEl.GetProperty("server").GetString()!,
                Username = proxyEl.TryGetProperty("username", out var u) ? u.GetString() : null,
                Password = proxyEl.TryGetProperty("password", out var p) ? p.GetString() : null,
                Bypass = proxyEl.TryGetProperty("bypass", out var b) ? b.GetString() : null,
            };
        }

        if (options.TimeoutMs.HasValue)
            launchOptions.Timeout = options.TimeoutMs.Value;

        if (options.SlowMo.HasValue)
            launchOptions.SlowMo = options.SlowMo.Value;

        return launchOptions;
    }

    /// <summary>
    /// Parses the JSON output into BrowserTypeLaunchPersistentContextOptions.
    /// </summary>
    public static BrowserTypeLaunchPersistentContextOptions ParsePersistent(
        string json, string userDataDir, CamoufoxOptions options)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var launchOptions = new BrowserTypeLaunchPersistentContextOptions
        {
            ExecutablePath = root.GetProperty("executable_path").GetString(),
            Headless = root.TryGetProperty("headless", out var h) && h.GetBoolean(),
            Args = ParseStringArray(root, "args"),
            Env = ParseStringDictionary(root, "env"),
            FirefoxUserPrefs = ParseMixedDictionary(root, "firefox_user_prefs"),
        };

        if (root.TryGetProperty("proxy", out var proxyEl) && proxyEl.ValueKind == JsonValueKind.Object)
        {
            launchOptions.Proxy = new Proxy
            {
                Server = proxyEl.GetProperty("server").GetString()!,
                Username = proxyEl.TryGetProperty("username", out var u) ? u.GetString() : null,
                Password = proxyEl.TryGetProperty("password", out var p) ? p.GetString() : null,
                Bypass = proxyEl.TryGetProperty("bypass", out var b) ? b.GetString() : null,
            };
        }

        if (options.TimeoutMs.HasValue)
            launchOptions.Timeout = options.TimeoutMs.Value;

        if (options.SlowMo.HasValue)
            launchOptions.SlowMo = options.SlowMo.Value;

        return launchOptions;
    }

    private static IEnumerable<string>? ParseStringArray(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out var el) || el.ValueKind != JsonValueKind.Array)
            return null;

        return el.EnumerateArray()
            .Where(e => e.ValueKind == JsonValueKind.String)
            .Select(e => e.GetString()!)
            .ToList();
    }

    private static IEnumerable<KeyValuePair<string, string>>? ParseStringDictionary(
        JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out var el) || el.ValueKind != JsonValueKind.Object)
            return null;

        return el.EnumerateObject()
            .Select(p => KeyValuePair.Create(p.Name, p.Value.ToString()))
            .ToList();
    }

    private static IDictionary<string, object>? ParseMixedDictionary(
        JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out var el) || el.ValueKind != JsonValueKind.Object)
            return null;

        var dict = new Dictionary<string, object>();
        foreach (var prop in el.EnumerateObject())
        {
            dict[prop.Name] = prop.Value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number when prop.Value.TryGetInt32(out var i) => i,
                JsonValueKind.Number => prop.Value.GetDouble(),
                JsonValueKind.String => prop.Value.GetString()!,
                _ => prop.Value.GetRawText(),
            };
        }
        return dict;
    }
}
