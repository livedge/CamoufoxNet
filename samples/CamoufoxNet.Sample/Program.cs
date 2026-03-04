using CamoufoxNet;
using Microsoft.Playwright;

var options = new CamoufoxOptions
{
    Headless = true,
    BlockImages = true,
    BlockWebRtc = true,
    Humanize = true,
    Window = (1280, 720),
    Locales = ["sk-SK", "sk"],
    EnableCache = true,
};

Console.WriteLine("Launching with: headless, block_images, block_webrtc, humanize, 1280x720, sk-SK locale, cache");

await using var camoufox = await Camoufox.CreateAsync(options);
var page = await camoufox.NewPageAsync();

await page.GotoAsync("https://www.nike.sk", new PageGotoOptions
{
    WaitUntil = WaitUntilState.NetworkIdle,
});

var title = await page.TitleAsync();
var url = page.Url;
var lang = await page.EvaluateAsync<string>("() => document.documentElement.lang || 'n/a'");
var ua = await page.EvaluateAsync<string>("() => navigator.userAgent");
var webrtcLeaks = await page.EvaluateAsync<bool>("""
    () => {
        try { new RTCPeerConnection(); return true; }
        catch { return false; }
    }
""");
var imgCount = await page.EvaluateAsync<int>("""
    () => Array.from(document.querySelectorAll('img'))
               .filter(i => i.naturalWidth > 0
                   && i.src.startsWith('http')
                   && !i.src.endsWith('.svg')).length
""");
var innerSize = await page.EvaluateAsync<string>("() => `${window.innerWidth}x${window.innerHeight}`");

Console.WriteLine($"  URL:          {url}");
Console.WriteLine($"  Title:        {title}");
Console.WriteLine($"  Lang:         {lang}");
Console.WriteLine($"  User-Agent:   {ua}");
Console.WriteLine($"  WebRTC:       {(webrtcLeaks ? "available (leak possible)" : "blocked")}");
Console.WriteLine($"  Raster imgs:  {imgCount}");
Console.WriteLine($"  Viewport:     {innerSize}");
Console.WriteLine();

// Verify expectations
var pass = true;
void Check(string name, bool ok)
{
    Console.WriteLine($"  [{(ok ? "PASS" : "FAIL")}] {name}");
    if (!ok) pass = false;
}

Check("Page loaded (has title)", !string.IsNullOrEmpty(title));
Check("Headless (no visible window)", true); // if we got here headless worked
Check("User-Agent is Firefox", ua.Contains("Firefox"));
Check("WebRTC blocked", !webrtcLeaks);
Check("Images blocked (0 rendered)", imgCount == 0);

Console.WriteLine();
Console.WriteLine(pass ? "All checks passed." : "Some checks FAILED.");
