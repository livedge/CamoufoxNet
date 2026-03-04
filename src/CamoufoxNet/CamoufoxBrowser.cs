using Microsoft.Playwright;

namespace CamoufoxNet;

/// <summary>
/// Owns a Playwright instance and browser, disposing both on cleanup.
/// </summary>
public sealed class CamoufoxBrowser : IAsyncDisposable, IDisposable
{
    public IBrowser Browser { get; }
    public IPlaywright Playwright { get; }

    internal CamoufoxBrowser(IPlaywright playwright, IBrowser browser)
    {
        Playwright = playwright;
        Browser = browser;
    }

    /// <summary>Creates a new page in a new browser context.</summary>
    public async Task<IPage> NewPageAsync()
    {
        var context = await Browser.NewContextAsync();
        return await context.NewPageAsync();
    }

    /// <summary>Creates a new browser context.</summary>
    public Task<IBrowserContext> NewContextAsync()
        => Browser.NewContextAsync();

    public async ValueTask DisposeAsync()
    {
        try { await Browser.CloseAsync(); } catch { /* best effort */ }
        Playwright.Dispose();
    }

    public void Dispose()
    {
        try { Browser.CloseAsync().GetAwaiter().GetResult(); } catch { /* best effort */ }
        Playwright.Dispose();
    }
}
