# SharpAapt

Read Android APK metadata from .NET. SharpAapt runs the Android SDK's `aapt2` or `aapt` tool against an APK and gives you the results as typed objects: package name and version, SDK levels, app labels and icons, permissions, features, locales, screen densities, native ABIs, the string pool, resource configurations and compiled XML.

Targets .NET Standard 2.0, so it runs on .NET Framework 4.6.1+, .NET Core 2.0+ and .NET 5 and later.

## Requirements

The Android SDK **build-tools** must be installed on the machine that runs your code. They ship `aapt2` and `aapt`. SharpAapt calls them; it does not bundle them.

## Quick start

```csharp
using SharpAapt;

var client = new AaptClient
{
    // Finds aapt2 (or aapt) in the newest build-tools under ANDROID_HOME,
    // ANDROID_SDK_ROOT, or the default SDK folder for your OS.
    AaptPath = AaptLocator.Find()
        ?? throw new InvalidOperationException("Android build-tools not found."),
};

ApkBadging apk = client.GetApkBadging("app.apk");

Console.WriteLine(apk.PackageName);            // com.example.app
Console.WriteLine(apk.VersionName);            // 1.4.2
Console.WriteLine(apk.VersionCode);            // 142
Console.WriteLine(apk.SdkVersionLevel);        // 24  (minSdkVersion)
Console.WriteLine(apk.TargetSdkVersionLevel);  // 34
Console.WriteLine(apk.ApplicationLabel);       // My App
Console.WriteLine(string.Join(", ", apk.NativeCode)); // arm64-v8a, armeabi-v7a
```

You can also set `AaptPath` to a specific `aapt2` or `aapt` executable. A shared instance is available as `AaptClient.Instance`.

## What you can read

| Method | Returns |
| --- | --- |
| `GetApkBadging` | `ApkBadging`: package, versions, SDK levels, labels (per locale), icons (per density), permissions, features, screens, locales, densities, native ABIs |
| `GetApkPermissions` | Permission names the APK requests |
| `GetApkStrings` | `ApkStrings`: the resource string pool |
| `GetApkConfigurations` | Resource configurations, such as `de`, `sw600dp` or `night` |
| `GetApkXmlTree` | Compiled XML tree of `AndroidManifest.xml`, or any XML file you name |
| `GetApkXmlStrings` | String pool of `AndroidManifest.xml`, or any XML file you name |

Each also has a raw-text version (for example `GetBadgingString`) that returns aapt's output unparsed.

## Async, timeouts and cancellation

Every method has an `Async` version that accepts a `CancellationToken`. Cancelling stops the running aapt process.

```csharp
client.Timeout = TimeSpan.FromSeconds(30); // default is 2 minutes

using var cts = new CancellationTokenSource();
ApkBadging apk = await client.GetApkBadgingAsync("app.apk", cts.Token);
```

## Errors

| Situation | Exception |
| --- | --- |
| `AaptPath` is not set | `InvalidOperationException` |
| aapt fails, for example a missing or corrupt APK | `AaptException`, with `ExitCode` and `StandardError` |
| A call runs longer than `Timeout` | `TimeoutException` |
| The token is cancelled | `OperationCanceledException` |

## Missing values

Not every APK declares everything. When the APK doesn't include a value, the property is `null` (for example `TargetSdkVersionLevel` or `ApplicationLabel`) or an empty collection (for example `NativeCode` or `Densities`).

## Mocking

`AaptClient` implements `IAaptClient`, so you can substitute it in tests.

## Links

- [Source code and issues](https://github.com/cprice70/SharpAapt)
- [License (MIT)](https://github.com/cprice70/SharpAapt/blob/master/LICENSE)
