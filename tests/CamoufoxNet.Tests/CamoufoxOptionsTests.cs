using CamoufoxNet.Internal;
using System.Text.Json;
using Xunit;

namespace CamoufoxNet.Tests;

public class CamoufoxOptionsTests
{
    [Fact]
    public void SerializeOptions_DefaultOptions_ContainsHeadlessFalse()
    {
        var options = new CamoufoxOptions();
        var json = PythonInterop.SerializeOptions(options);
        using var doc = JsonDocument.Parse(json);

        Assert.False(doc.RootElement.GetProperty("headless").GetBoolean());
    }

    [Fact]
    public void SerializeOptions_Headless_SetsTrue()
    {
        var options = new CamoufoxOptions { Headless = true };
        var json = PythonInterop.SerializeOptions(options);
        using var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.GetProperty("headless").GetBoolean());
    }

    [Fact]
    public void SerializeOptions_BlockFlags_IncludedWhenTrue()
    {
        var options = new CamoufoxOptions
        {
            BlockImages = true,
            BlockWebRtc = true,
            BlockWebGl = true,
            DisableCoop = true,
        };
        var json = PythonInterop.SerializeOptions(options);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.GetProperty("block_images").GetBoolean());
        Assert.True(root.GetProperty("block_webrtc").GetBoolean());
        Assert.True(root.GetProperty("block_webgl").GetBoolean());
        Assert.True(root.GetProperty("disable_coop").GetBoolean());
    }

    [Fact]
    public void SerializeOptions_BlockFlags_OmittedWhenFalse()
    {
        var options = new CamoufoxOptions();
        var json = PythonInterop.SerializeOptions(options);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.False(root.TryGetProperty("block_images", out _));
        Assert.False(root.TryGetProperty("block_webrtc", out _));
        Assert.False(root.TryGetProperty("block_webgl", out _));
        Assert.False(root.TryGetProperty("disable_coop", out _));
    }

    [Fact]
    public void SerializeOptions_Proxy_SerializesCorrectly()
    {
        var options = new CamoufoxOptions
        {
            Proxy = new ProxyOptions
            {
                Server = "http://proxy:8080",
                Username = "user",
                Password = "pass",
            }
        };
        var json = PythonInterop.SerializeOptions(options);
        using var doc = JsonDocument.Parse(json);
        var proxy = doc.RootElement.GetProperty("proxy");

        Assert.Equal("http://proxy:8080", proxy.GetProperty("server").GetString());
        Assert.Equal("user", proxy.GetProperty("username").GetString());
        Assert.Equal("pass", proxy.GetProperty("password").GetString());
    }

    [Fact]
    public void SerializeOptions_Window_SerializesAsTuple()
    {
        var options = new CamoufoxOptions { Window = (1920, 1080) };
        var json = PythonInterop.SerializeOptions(options);
        using var doc = JsonDocument.Parse(json);
        var window = doc.RootElement.GetProperty("window");

        Assert.Equal(1920, window[0].GetInt32());
        Assert.Equal(1080, window[1].GetInt32());
    }

    [Fact]
    public void SerializeOptions_WebGlConfig_SerializesAsTuple()
    {
        var options = new CamoufoxOptions
        {
            WebGlConfig = ("Mesa", "llvmpipe")
        };
        var json = PythonInterop.SerializeOptions(options);
        using var doc = JsonDocument.Parse(json);
        var config = doc.RootElement.GetProperty("webgl_config");

        Assert.Equal("Mesa", config[0].GetString());
        Assert.Equal("llvmpipe", config[1].GetString());
    }

    [Fact]
    public void SerializeOptions_TargetOs_SerializesAsList()
    {
        var options = new CamoufoxOptions { TargetOs = ["windows", "macos"] };
        var json = PythonInterop.SerializeOptions(options);
        using var doc = JsonDocument.Parse(json);
        var os = doc.RootElement.GetProperty("os");

        Assert.Equal(2, os.GetArrayLength());
        Assert.Equal("windows", os[0].GetString());
        Assert.Equal("macos", os[1].GetString());
    }

    [Fact]
    public void SerializeOptions_Humanize_Bool()
    {
        var options = new CamoufoxOptions { Humanize = true };
        var json = PythonInterop.SerializeOptions(options);
        using var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.GetProperty("humanize").GetBoolean());
    }

    [Fact]
    public void SerializeOptions_Humanize_WithMaxDuration()
    {
        var options = new CamoufoxOptions { Humanize = true, HumanizeMaxDurationSeconds = 2.5 };
        var json = PythonInterop.SerializeOptions(options);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal(2.5, doc.RootElement.GetProperty("humanize").GetDouble());
    }

    [Fact]
    public void SerializeOptions_GeoIpAutoDetect_SetsTrue()
    {
        var options = new CamoufoxOptions { GeoIpAutoDetect = true };
        var json = PythonInterop.SerializeOptions(options);
        using var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.GetProperty("geoip").GetBoolean());
    }

    [Fact]
    public void SerializeOptions_GeoIp_SetsString()
    {
        var options = new CamoufoxOptions { GeoIp = "US" };
        var json = PythonInterop.SerializeOptions(options);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal("US", doc.RootElement.GetProperty("geoip").GetString());
    }

    [Fact]
    public void SerializeOptions_Screen_SerializesConstraints()
    {
        var options = new CamoufoxOptions
        {
            Screen = new ScreenConstraints(MinWidth: 1024, MaxWidth: 1920)
        };
        var json = PythonInterop.SerializeOptions(options);
        using var doc = JsonDocument.Parse(json);
        var screen = doc.RootElement.GetProperty("screen");

        Assert.Equal(1024, screen.GetProperty("min_width").GetInt32());
        Assert.Equal(1920, screen.GetProperty("max_width").GetInt32());
        Assert.False(screen.TryGetProperty("min_height", out _));
    }

    [Fact]
    public void SerializeOptions_ExtraArgs_Included()
    {
        var options = new CamoufoxOptions { Args = ["--no-remote"] };
        var json = PythonInterop.SerializeOptions(options);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal("--no-remote", doc.RootElement.GetProperty("args")[0].GetString());
    }

    [Fact]
    public void SerializeOptions_MainWorldEval_Included()
    {
        var options = new CamoufoxOptions { MainWorldEval = true };
        var json = PythonInterop.SerializeOptions(options);
        using var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.GetProperty("main_world_eval").GetBoolean());
    }
}
