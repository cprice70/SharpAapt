using System;
using System.Linq;
using System.Threading.Tasks;

namespace SharpAapt.Demo
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 2)
            {
                Console.Error.WriteLine("Usage: SharpAapt.Demo <path-to-apk> [path-to-aapt-or-aapt2]");
                return 2;
            }

            var apkPath = args[0];
            var aaptPath = args.Length > 1 ? args[1] : AaptLocator.Find();
            if (aaptPath == null)
            {
                Console.Error.WriteLine("aapt not found. Set ANDROID_HOME or pass the aapt path as the second argument.");
                return 1;
            }

            var client = new AaptClient { AaptPath = aaptPath };
            Console.WriteLine($"Using {aaptPath}");

            try
            {
                var badging = await client.GetApkBadgingAsync(apkPath);
                Console.WriteLine();
                Console.WriteLine("**** Badging ****");
                Console.WriteLine($"Package:      {badging.PackageName}");
                Console.WriteLine($"Version:      {badging.VersionName} ({badging.VersionCode})");
                Console.WriteLine($"Min SDK:      {badging.SdkVersionLevel}");
                Console.WriteLine($"Target SDK:   {badging.TargetSdkVersionLevel}");
                Console.WriteLine($"Label:        {badging.ApplicationLabel}");
                Console.WriteLine($"Native code:  {string.Join(", ", badging.NativeCode)}");

                Console.WriteLine();
                Console.WriteLine("**** Permissions ****");
                foreach (var permission in await client.GetApkPermissionsAsync(apkPath))
                    Console.WriteLine(permission);

                Console.WriteLine();
                Console.WriteLine("**** Configurations ****");
                Console.WriteLine(string.Join(", ", await client.GetApkConfigurationsAsync(apkPath)));

                var strings = await client.GetApkStringsAsync(apkPath);
                Console.WriteLine();
                Console.WriteLine($"**** Strings ({strings?.Strings.Count ?? 0}) ****");
                foreach (var pair in strings?.Strings.Take(10) ?? Enumerable.Empty<System.Collections.Generic.KeyValuePair<string, string>>())
                    Console.WriteLine($"{pair.Key}: {pair.Value}");

                return 0;
            }
            catch (AaptException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}
