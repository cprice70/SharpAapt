using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SharpAapt
{
    /// <summary>
    /// Parsed output of <c>aapt dump badging</c> or <c>aapt2 dump badging</c>.
    /// Values whose line is missing from the output are <c>null</c>, or empty for collections.
    /// </summary>
    public class ApkBadging
    {
        public ApkBadging(string apkBadgingString)
        {
            if (apkBadgingString == null) throw new ArgumentNullException(nameof(apkBadgingString));

            var results = apkBadgingString
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .ToList();

            string Line(string prefix) => results.FirstOrDefault(line => line.StartsWith(prefix, StringComparison.Ordinal));
            IEnumerable<string> Lines(string prefix) => results.Where(line => line.StartsWith(prefix, StringComparison.Ordinal));

            // aapt prints "sdkVersion:", aapt2 prints "minSdkVersion:".
            SdkVersionLevel = ParseInt(FirstQuoted(Line("sdkVersion:") ?? Line("minSdkVersion:")));
            TargetSdkVersionLevel = ParseInt(FirstQuoted(Line("targetSdkVersion:")));
            InstallLocation = FirstQuoted(Line("install-location:"));

            var packageLine = Line("package:");
            if (packageLine != null)
            {
                PackageName = RegexHelpers.Attribute(packageLine, "name");
                VersionName = RegexHelpers.Attribute(packageLine, "versionName");
                VersionCode = ParseInt(RegexHelpers.Attribute(packageLine, "versionCode"));
            }

            // application-label:'My App' or application-label-de:'Meine App'
            foreach (var line in Lines("application-label"))
            {
                var parts = line.Remove(0, "application-label".Length).Split(new[] { ':' }, 2);
                if (parts.Length < 2) continue;
                var culture = string.IsNullOrEmpty(parts[0]) ? "culture-neutral" : parts[0].TrimStart('-');
                ApplicationLabels[culture] = RegexHelpers.FirstQuotedValue(parts[1]);
            }

            // uses-permission: name='android.permission.BLUETOOTH'
            foreach (var line in Lines("uses-permission"))
                Permissions.Add(RegexHelpers.Attribute(line, "name") ?? RegexHelpers.FirstQuotedValue(line));

            // application-icon-160:'res/icon.png'
            foreach (var line in Lines("application-icon-"))
            {
                var parts = line.Remove(0, "application-icon-".Length).Split(new[] { ':' }, 2);
                if (parts.Length < 2) continue;
                ApplicationIcons[parts[0]] = RegexHelpers.FirstQuotedValue(parts[1]);
            }

            var applicationLine = Line("application:");
            if (applicationLine != null)
            {
                ApplicationLabel = RegexHelpers.Attribute(applicationLine, "label");
                ApplicationIcon = RegexHelpers.Attribute(applicationLine, "icon");
            }

            ScreenSizes = QuotedList(Line("supports-screens:"));
            Locales = QuotedList(Line("locales:"));
            NativeCode = QuotedList(Line("native-code:"));
            Densities = QuotedList(Line("densities:"))
                .Select(ParseInt)
                .Where(d => d.HasValue)
                .Select(d => d.Value)
                .ToList();

            var anyDensity = FirstQuoted(Line("supports-any-density:"));
            if (bool.TryParse(anyDensity, out var supportsAnyDensity))
                SupportsAnyDensity = supportsAnyDensity;

            var featureGroupLine = Line("feature-group:");
            AppFeatureGroup = new FeatureGroup
            {
                Label = featureGroupLine == null ? string.Empty : RegexHelpers.Attribute(featureGroupLine, "label") ?? string.Empty
            };

            foreach (var line in Lines("uses-feature:"))
                AppFeatureGroup.Features.Add(ParseFeature(line, implied: false));

            foreach (var line in Lines("uses-implied-feature:"))
                AppFeatureGroup.Features.Add(ParseFeature(line, implied: true));
        }

        private static FeatureGroup.Feature ParseFeature(string line, bool implied) => new FeatureGroup.Feature
        {
            Name = RegexHelpers.Attribute(line, "name"),
            Reason = RegexHelpers.Attribute(line, "reason") ?? string.Empty,
            Implied = implied
        };

        private static string FirstQuoted(string line) => line == null ? null : RegexHelpers.FirstQuotedValue(line);

        private static List<string> QuotedList(string line) => line == null ? new List<string>() : RegexHelpers.QuotedValues(line);

        private static int? ParseInt(string value) =>
            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : (int?)null;

        public int? SdkVersionLevel { get; }

        public int? TargetSdkVersionLevel { get; }

        public string InstallLocation { get; }

        public string PackageName { get; }

        public string VersionName { get; }

        public int? VersionCode { get; }

        public string ApplicationLabel { get; }

        public string ApplicationIcon { get; }

        public Dictionary<string, string> ApplicationLabels { get; } = new Dictionary<string, string>();

        public Dictionary<string, string> ApplicationIcons { get; } = new Dictionary<string, string>();

        public List<string> Permissions { get; } = new List<string>();

        public List<string> ScreenSizes { get; }

        public List<int> Densities { get; }

        public List<string> Locales { get; }

        public bool? SupportsAnyDensity { get; }

        /// <summary>
        /// Native ABIs the APK ships code for, e.g. <c>arm64-v8a</c>. Empty for pure managed/Java APKs.
        /// </summary>
        public List<string> NativeCode { get; }

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
