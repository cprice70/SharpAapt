# SharpAapt

SharpAapt is a .NET library that runs the Android build tools `aapt` or `aapt2` against an APK and parses the output: package name and version, SDK levels, labels, icons, permissions, features, locales, densities, native ABIs, string pools, configurations and compiled XML.

It targets .NET Standard 2.0, so it works on .NET Framework 4.6.1+, .NET Core 2.0+ and .NET 5+.

## Install

```bash
dotnet add package sharpaapt
```

You also need the Android SDK build-tools installed, which ship `aapt` and `aapt2`.

## Usage

```csharp
using SharpAapt;

var client = new AaptClient
{
    // Finds aapt2 (or aapt) in the newest build-tools of ANDROID_HOME, ANDROID_SDK_ROOT,
    // or the default SDK location for your OS. You can also set a path directly.
    AaptPath = AaptLocator.Find() ?? throw new InvalidOperationException("Android build-tools not found"),
};

ApkBadging badging = client.GetApkBadging("app.apk");
Console.WriteLine($"{badging.PackageName} {badging.VersionName} ({badging.VersionCode})");
Console.WriteLine($"minSdk {badging.SdkVersionLevel}, targetSdk {badging.TargetSdkVersionLevel}");
Console.WriteLine($"Label: {badging.ApplicationLabel}");
Console.WriteLine($"ABIs: {string.Join(", ", badging.NativeCode)}");

IEnumerable<string> permissions = client.GetApkPermissions("app.apk");
ApkStrings strings = client.GetApkStrings("app.apk");
IEnumerable<string> configurations = client.GetApkConfigurations("app.apk");
string manifestTree = client.GetApkXmlTree("app.apk"); // AndroidManifest.xml by default
```

A shared instance is also available as `AaptClient.Instance`.

### Async, timeouts and cancellation

Every method has an `...Async` version that takes a `CancellationToken`. Each call to `aapt` is killed if it runs longer than `Timeout` (2 minutes by default).

```csharp
client.Timeout = TimeSpan.FromSeconds(30);

using var cts = new CancellationTokenSource();
ApkBadging badging = await client.GetApkBadgingAsync("app.apk", cts.Token);
```

| Situation | Exception |
| --- | --- |
| `AaptPath` not set | `InvalidOperationException` |
| `aapt` exits with a non-zero code (bad path, corrupt APK) | `AaptException`, with `ExitCode` and `StandardError` |
| Call runs longer than `Timeout` | `TimeoutException` |
| Token is cancelled | `OperationCanceledException` |

### Missing values

Not every APK has every badging line. When a line is missing, the matching `ApkBadging` property is `null` (for example `TargetSdkVersionLevel`, `ApplicationLabel`, `SupportsAnyDensity`) or an empty collection (for example `NativeCode`, `Densities`, `ApplicationIcons`).

### aapt vs aapt2

`aapt` is deprecated; `AaptLocator.Find()` prefers `aapt2`. Both are supported. SharpAapt detects `aapt2` from the executable name and passes XML dump arguments in the order it expects.

## Development

```bash
dotnet test SharpAapt.Tests
```

- Parser tests use saved output in `SharpAapt.Tests/Fixtures`.
- `AaptClientTests` run against a fake `aapt` shell script, so they need no Android SDK. They are skipped on Windows.
- `IntegrationTests` run the real `aapt` and `aapt2` against `Fixtures/test.apk`. Each is skipped if that tool isn't found.

`Fixtures/test.apk` is built from `SharpAapt.Tests/TestApp`. To rebuild it:

```bash
SharpAapt.Tests/TestApp/build.sh
```

The script uses `ANDROID_HOME`, or takes the SDK path as its first argument, and needs the `android-34` platform installed.

### Releasing

CI runs on every push and pull request to `master`. Pushing a tag like `v0.2.0` builds and packs that version and pushes it to nuget.org using [trusted publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing), so no API key is stored. This needs a nuget.org trusted publishing policy for this repository with workflow file `ci.yml`, and a `NUGET_USER` repository secret holding the nuget.org profile name.

## License

MIT
