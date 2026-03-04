using System.Text.Json;
using CamoufoxNet.Internal;
using Xunit;

namespace CamoufoxNet.Tests;

public class LaunchOptionsParserTests
{
    private static readonly string SampleJson = JsonSerializer.Serialize(new
    {
        executable_path = "/usr/bin/camoufox",
        headless = false,
        args = new[] { "--no-remote", "-profile", "/tmp/profile" },
        env = new Dictionary<string, string>
        {
            ["CAMOU_CONFIG_1"] = "eyJ0ZXN0IjogdHJ1ZX0=",
            ["MOZ_HEADLESS"] = "0",
        },
        firefox_user_prefs = new Dictionary<string, object>
        {
            ["network.proxy.type"] = 1,
            ["browser.cache.disk.enable"] = false,
            ["general.useragent.override"] = "Mozilla/5.0",
        },
    });

    [Fact]
    public void Parse_ExecutablePath_Parsed()
    {
        var result = LaunchOptionsParser.Parse(SampleJson, new CamoufoxOptions());
        Assert.Equal("/usr/bin/camoufox", result.ExecutablePath);
    }

    [Fact]
    public void Parse_Headless_Parsed()
    {
        var result = LaunchOptionsParser.Parse(SampleJson, new CamoufoxOptions());
        Assert.False(result.Headless);
    }

    [Fact]
    public void Parse_Args_Parsed()
    {
        var result = LaunchOptionsParser.Parse(SampleJson, new CamoufoxOptions());
        var args = result.Args!.ToList();
        Assert.Equal(3, args.Count);
        Assert.Contains("--no-remote", args);
    }

    [Fact]
    public void Parse_Env_Parsed()
    {
        var result = LaunchOptionsParser.Parse(SampleJson, new CamoufoxOptions());
        var env = result.Env!.ToList();
        Assert.Contains(env, kv => kv.Key == "CAMOU_CONFIG_1");
    }

    [Fact]
    public void Parse_FirefoxUserPrefs_MixedTypes()
    {
        var result = LaunchOptionsParser.Parse(SampleJson, new CamoufoxOptions());
        var prefs = result.FirefoxUserPrefs!.ToDictionary(kv => kv.Key, kv => kv.Value);
        Assert.Equal(1, prefs["network.proxy.type"]);
        Assert.Equal(false, prefs["browser.cache.disk.enable"]);
        Assert.Equal("Mozilla/5.0", prefs["general.useragent.override"]);
    }

    [Fact]
    public void Parse_WithProxy_ParsesProxy()
    {
        var json = JsonSerializer.Serialize(new
        {
            executable_path = "/usr/bin/camoufox",
            headless = false,
            args = Array.Empty<string>(),
            env = new Dictionary<string, string>(),
            firefox_user_prefs = new Dictionary<string, object>(),
            proxy = new
            {
                server = "http://proxy:8080",
                username = "user",
                password = "pass",
            },
        });

        var result = LaunchOptionsParser.Parse(json, new CamoufoxOptions());
        Assert.NotNull(result.Proxy);
        Assert.Equal("http://proxy:8080", result.Proxy!.Server);
        Assert.Equal("user", result.Proxy.Username);
    }

    [Fact]
    public void Parse_WithTimeout_SetsTimeout()
    {
        var options = new CamoufoxOptions { TimeoutMs = 30000 };
        var result = LaunchOptionsParser.Parse(SampleJson, options);
        Assert.Equal(30000f, result.Timeout);
    }

    [Fact]
    public void Parse_WithSlowMo_SetsSlowMo()
    {
        var options = new CamoufoxOptions { SlowMo = 100 };
        var result = LaunchOptionsParser.Parse(SampleJson, options);
        Assert.Equal(100f, result.SlowMo);
    }

    [Fact]
    public void ParsePersistent_ReturnsCorrectType()
    {
        var options = new CamoufoxOptions();
        var result = LaunchOptionsParser.ParsePersistent(SampleJson, "/tmp/userdata", options);
        Assert.Equal("/usr/bin/camoufox", result.ExecutablePath);
    }

    [Fact]
    public void Parse_HeadlessTrue_Parsed()
    {
        var json = JsonSerializer.Serialize(new
        {
            executable_path = "/usr/bin/camoufox",
            headless = true,
            args = Array.Empty<string>(),
            env = new Dictionary<string, string>(),
            firefox_user_prefs = new Dictionary<string, object>(),
        });

        var result = LaunchOptionsParser.Parse(json, new CamoufoxOptions());
        Assert.True(result.Headless);
    }
}
