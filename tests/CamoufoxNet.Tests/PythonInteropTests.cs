using CamoufoxNet.Internal;
using Xunit;

namespace CamoufoxNet.Tests;

public class PythonInteropTests
{
    [Fact]
    public void GetBridgeScriptPath_ReturnsValidPath()
    {
        var path = PythonInterop.GetBridgeScriptPath();

        Assert.NotEmpty(path);
        Assert.True(File.Exists(path), $"Bridge script should exist at: {path}");
        Assert.EndsWith("launch_bridge.py", path);
    }

    [Fact]
    public void GetBridgeScriptPath_ContainsPythonCode()
    {
        var path = PythonInterop.GetBridgeScriptPath();
        var content = File.ReadAllText(path);

        Assert.Contains("launch_options", content);
        Assert.Contains("json.dump", content);
    }

    [Fact]
    public void GetBridgeScriptPath_IsCached()
    {
        var path1 = PythonInterop.GetBridgeScriptPath();
        var path2 = PythonInterop.GetBridgeScriptPath();

        Assert.Equal(path1, path2);
    }

    [Fact]
    public void SerializeOptions_ProducesValidJson()
    {
        var options = new CamoufoxOptions
        {
            Headless = true,
            BlockImages = true,
            Proxy = new ProxyOptions { Server = "http://localhost:8080" },
            Window = (1920, 1080),
            Locales = ["en-US", "en"],
            Fonts = ["Arial", "Helvetica"],
        };

        var json = PythonInterop.SerializeOptions(options);

        // Should be valid JSON
        Assert.NotEmpty(json);
        var doc = System.Text.Json.JsonDocument.Parse(json);
        Assert.NotNull(doc);
    }
}
