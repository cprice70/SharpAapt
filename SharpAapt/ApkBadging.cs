using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpAapt
{
    public class ApkBadging
    {
        public ApkBadging(string apkBadgingString)
        {
            var results = apkBadgingString
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .ToList();

            // Get SdkVersion
            var versionLine = results.First(line => line.StartsWith("sdkVersion:"));
            SdkVersionLevel = Convert.ToInt32(RegexHelpers.FirstQuotedValue(versionLine));

            // Get TargetSdkVersion
            var targetLine = results.First(line => line.StartsWith("targetSdkVersion:"));
            TargetSdkVersionLevel = Convert.ToInt32(RegexHelpers.FirstQuotedValue(targetLine));

            // Get InstallLocation
            var installLine = results.FirstOrDefault(line => line.StartsWith("install-location:"));
            if (!string.IsNullOrEmpty(installLine))
                InstallLocation = RegexHelpers.FirstQuotedValue(installLine);

            // Get Package Info
            var packageLine = results.First(line => line.StartsWith("package:"));
            PackageName = RegexHelpers.Attribute(packageLine, "name");
            VersionName = RegexHelpers.Attribute(packageLine, "versionName");
            VersionCode = Convert.ToInt32(RegexHelpers.Attribute(packageLine, "versionCode"));

            // Get Application Labels, e.g. application-label:'My App' or application-label-de:'Meine App'
            foreach (var line in results.Where(line => line.StartsWith("application-label")))
            {
                var parts = line.Remove(0, "application-label".Length).Split(new[] { ':' }, 2);
                if (parts.Length < 2) continue;
                var culture = string.IsNullOrEmpty(parts[0]) ? "culture-neutral" : parts[0].TrimStart('-');
                ApplicationLabels[culture] = RegexHelpers.FirstQuotedValue(parts[1]);
            }

            // Permissions, e.g. uses-permission: name='android.permission.BLUETOOTH'
            foreach (var line in results.Where(line => line.StartsWith("uses-permission")))
            {
                var name = RegexHelpers.Attribute(line, "name") ?? RegexHelpers.FirstQuotedValue(line);
                Permissions.Add(name);
            }

            // Application Icons, e.g. application-icon-160:'res/icon.png'
            foreach (var line in results.Where(line => line.StartsWith("application-icon-")))
            {
                var parts = line.Remove(0, "application-icon-".Length).Split(new[] { ':' }, 2);
                if (parts.Length < 2) continue;
                ApplicationIcons[parts[0]] = RegexHelpers.FirstQuotedValue(parts[1]);
            }

            // Application Info
            var applicationLine = results.First(line => line.StartsWith("application:"));
            ApplicationLabel = RegexHelpers.Attribute(applicationLine, "label");
            ApplicationIcon = RegexHelpers.Attribute(applicationLine, "icon");

            var screensLine = results.First(line => line.StartsWith("supports-screens:"));
            ScreenSizes = RegexHelpers.QuotedValues(screensLine);

            var densitiesLine = results.First(line => line.StartsWith("densities:"));
            Densities = RegexHelpers.QuotedValues(densitiesLine).Select(d => Convert.ToInt32(d)).ToList();

            var localesLine = results.First(line => line.StartsWith("locales:"));
            Locales = RegexHelpers.QuotedValues(localesLine);

            var anyDensityLine = results.First(line => line.StartsWith("supports-any-density:"));
            SupportsAnyDensity = Convert.ToBoolean(RegexHelpers.FirstQuotedValue(anyDensityLine));

            var nativeCodeLine = results.FirstOrDefault(line => line.StartsWith("native-code:"));
            if (!string.IsNullOrEmpty(nativeCodeLine))
                NativeCode = RegexHelpers.FirstQuotedValue(nativeCodeLine);

            // Feature Group
            var featureGroupLine = results.First(line => line.StartsWith("feature-group:"));
            AppFeatureGroup = new FeatureGroup { Label = RegexHelpers.Attribute(featureGroupLine, "label") ?? string.Empty };

            foreach (var line in results.Where(line => line.StartsWith("uses-feature:")))
            {
                AppFeatureGroup.Features.Add(new FeatureGroup.Feature
                {
                    Name = RegexHelpers.Attribute(line, "name"),
                    Reason = RegexHelpers.Attribute(line, "reason") ?? string.Empty
                });
            }

            foreach (var line in results.Where(line => line.StartsWith("uses-implied-feature:")))
            {
                AppFeatureGroup.Features.Add(new FeatureGroup.Feature
                {
                    Name = RegexHelpers.Attribute(line, "name"),
                    Reason = RegexHelpers.Attribute(line, "reason") ?? string.Empty,
                    Implied = true
                });
            }
        }

        public int SdkVersionLevel { get; }

        public int TargetSdkVersionLevel { get; }

        public string InstallLocation { get; }

        public string PackageName { get; }

        public string VersionName { get; }

        public int VersionCode { get; }

        public string ApplicationLabel { get; }

        public string ApplicationIcon { get; }

        public Dictionary<string, string> ApplicationLabels { get; } = new Dictionary<string, string>();

        public Dictionary<string, string> ApplicationIcons { get; } = new Dictionary<string, string>();

        public List<string> Permissions { get; } = new List<string>();

        public List<string> ScreenSizes { get; }

        public List<int> Densities { get; }

        public List<string> Locales { get; }

        public bool SupportsAnyDensity { get; }

        public string NativeCode { get; }

        public FeatureGroup AppFeatureGroup { get; }
    }

    public class FeatureGroup
    {
        public struct Feature
        {
            public string Name { get; set; }
            public string Reason { get; set; }
            public bool Implied { get; set; }
        }

        public string Label { get; set; } = string.Empty;

        public List<Feature> Features { get; } = new List<Feature>();
    }
}
