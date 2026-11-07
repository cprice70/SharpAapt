using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SharpAapt
{
    /// <summary>
    /// Finds aapt or aapt2 in an installed Android SDK.
    /// </summary>
    public static class AaptLocator
    {
        /// <summary>
        /// Returns the path to aapt2 (or aapt, if <paramref name="preferAapt2"/> is false) from the newest
        /// build-tools version in the Android SDK, falling back to the other tool if the preferred one isn't
        /// installed. Looks in <c>ANDROID_HOME</c>, <c>ANDROID_SDK_ROOT</c>, then the default SDK location
        /// for the current OS. Returns <c>null</c> if neither tool is found.
        /// </summary>
        public static string Find(bool preferAapt2 = true) => Find(DefaultSdkRoots(), preferAapt2);

        internal static string Find(IEnumerable<string> sdkRoots, bool preferAapt2)
        {
            var roots = sdkRoots.ToList();
            var order = preferAapt2 ? new[] { "aapt2", "aapt" } : new[] { "aapt", "aapt2" };

            return order.Select(tool => FindTool(roots, tool)).FirstOrDefault(path => path != null);
        }

        internal static string FindTool(IEnumerable<string> sdkRoots, string toolName)
        {
            var fileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? toolName + ".exe" : toolName;

            foreach (var root in sdkRoots)
            {
                if (string.IsNullOrWhiteSpace(root)) continue;

                var buildTools = Path.Combine(root, "build-tools");
                if (!Directory.Exists(buildTools)) continue;

                var match = Directory.GetDirectories(buildTools)
                    .OrderByDescending(dir => ParseVersion(Path.GetFileName(dir)))
                    .Select(dir => Path.Combine(dir, fileName))
                    .FirstOrDefault(File.Exists);

                if (match != null) return match;
            }

            return null;
        }

        internal static IEnumerable<string> DefaultSdkRoots()
        {
            yield return Environment.GetEnvironmentVariable("ANDROID_HOME");
            yield return Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");

            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "Sdk");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                yield return Path.Combine(home, "Library", "Android", "sdk");
            else
                yield return Path.Combine(home, "Android", "Sdk");
        }

        // Build-tools folders are named like "35.0.0" or "36.0.0-rc1". Unparseable names sort last.
        private static Version ParseVersion(string name)
        {
            var numeric = name.Split('-')[0];
            return Version.TryParse(numeric, out var version) ? version : new Version(0, 0);
        }
    }
}
