using CamoufoxNet.Internal;
using Microsoft.Playwright;

namespace CamoufoxNet;

/// <summary>
/// Static facade for launching Camoufox browsers.
/// </summary>
public static class Camoufox
{
    /// <summary>
    /// Creates a CamoufoxBrowser that owns both the Playwright instance and the browser.
    /// Dispose the returned object to clean up all resources.
    /// </summary>
    public static async Task<CamoufoxBrowser> CreateAsync(
        CamoufoxOptions? options = null, CancellationToken ct = default)
    {
        var pw = await Microsoft.Playwright.Playwright.CreateAsync();
        try
        {
            var browser = await LaunchAsync(pw, options, ct);
            return new CamoufoxBrowser(pw, browser);
        }
        catch
        {
            pw.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Launches a Camoufox browser using an existing Playwright instance.
    /// The caller is responsible for disposing the Playwright instance.
    /// </summary>
    public static async Task<IBrowser> LaunchAsync(
        IPlaywright pw, CamoufoxOptions? options = null, CancellationToken ct = default)
    {
        var launchOptions = await BuildLaunchOptionsAsync(options, ct);
        return await pw.Firefox.LaunchAsync(launchOptions);
    }

    /// <summary>
    /// Launches a persistent browser context using an existing Playwright instance.
    /// </summary>
    public static async Task<IBrowserContext> LaunchPersistentContextAsync(
        IPlaywright pw, string userDataDir,
        CamoufoxOptions? options = null, CancellationToken ct = default)
    {
        options ??= new CamoufoxOptions();

        var json = await PythonInterop.GetLaunchOptionsJsonAsync(options, ct);
        var launchOptions = LaunchOptionsParser.ParsePersistent(json, userDataDir, options);
        return await pw.Firefox.LaunchPersistentContextAsync(userDataDir, launchOptions);
    }

    /// <summary>
    /// Builds Playwright launch options by calling the Python bridge, without launching a browser.
    /// Useful for inspecting or customizing the options before launch.
    /// </summary>
    public static async Task<BrowserTypeLaunchOptions> BuildLaunchOptionsAsync(
        CamoufoxOptions? options = null, CancellationToken ct = default)
    {
        options ??= new CamoufoxOptions();

        var json = await PythonInterop.GetLaunchOptionsJsonAsync(options, ct);
        return LaunchOptionsParser.Parse(json, options);
    }
}
