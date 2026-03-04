# CamoufoxNet

[![Build](https://github.com/livedge/CamoufoxNet/actions/workflows/build.yml/badge.svg)](https://github.com/livedge/CamoufoxNet/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/CamoufoxNet.svg)](https://www.nuget.org/packages/CamoufoxNet)
[![NuGet Downloads](https://img.shields.io/nuget/dt/CamoufoxNet.svg)](https://www.nuget.org/packages/CamoufoxNet)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

C# wrapper for [Camoufox](https://github.com/nicedayzhu/camoufox) anti-detect browser using Microsoft.Playwright.

## Prerequisites

- .NET 8.0+
- Python 3.10+ with `camoufox` package installed:

```bash
pip install camoufox
camoufox fetch
```

## Installation

```bash
dotnet add package CamoufoxNet
```

## Quick Start

```csharp
using CamoufoxNet;

await using var camoufox = await Camoufox.CreateAsync();
var page = await camoufox.NewPageAsync();
await page.GotoAsync("https://example.com");
```

## Options

```csharp
var options = new CamoufoxOptions
{
    Headless = true,
    BlockImages = true,
    BlockWebRtc = true,
    Humanize = true,
    Window = (1280, 720),
    Locales = ["sk-SK", "sk"],
    EnableCache = true,
    Proxy = new ProxyOptions
    {
        Server = "http://proxy:8080",
        Username = "user",
        Password = "pass",
    },
};

await using var camoufox = await Camoufox.CreateAsync(options);
```

## Using an Existing Playwright Instance

```csharp
using Microsoft.Playwright;

var pw = await Playwright.CreateAsync();
var browser = await Camoufox.LaunchAsync(pw, new CamoufoxOptions { Headless = true });
```

## Persistent Context

```csharp
var pw = await Playwright.CreateAsync();
var context = await Camoufox.LaunchPersistentContextAsync(pw, "./user-data");
```

## How It Works

1. C# builds a `CamoufoxOptions` object
2. Calls a Python bridge script via subprocess
3. Python calls `camoufox.utils.launch_options()` to generate fingerprint config
4. C# parses the result into Playwright `BrowserTypeLaunchOptions`
5. Launches the Camoufox Firefox binary via `playwright.Firefox.LaunchAsync()`
6. Returns a standard Playwright `IBrowser`

## License

MIT
