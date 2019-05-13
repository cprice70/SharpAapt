using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SharpAapt
{
    public class ApkBadging
    {
        public ApkBadging(string apkBadgingString)
        {
            var results = apkBadgingString.Split('\n');

            // Get SdkVersion
            var versionLine = results.First(line => line.Contains("sdkVersion:"));
            var regex = new Regex("\\d+");
            var match = regex.Match(versionLine);
            SdkVersionLevel = Convert.ToInt32(match.Value);

            // Get SdkVersion
            var targetLine = results.First(line => line.Contains("targetSdkVersion:"));
            match = regex.Match(targetLine);
            TargetSdkVersionLevel = Convert.ToInt32(match.Value);

            // Get InstallLocation
            var installLine = results.FirstOrDefault(line => line.Contains("install-location:"));
            if (!string.IsNullOrEmpty(installLine))
            {
                var installMatch = RegexHelpers.ValueRegex.Match(installLine);
                InstallLocation = installMatch.Value;
            }

            // Get Package Info
            var packageLine = results.First(line => line.Contains("package:"));
            var packageLineValue = packageLine.Split(':');
            var packageValues = packageLineValue[1].Split(' ');

            var packageName = packageValues.First(line => line.Contains("name"));
            var packageNameMatch = RegexHelpers.ValueRegex.Match(packageName);
            PackageName = packageNameMatch.Value;

            var version = packageValues.First(line => line.Contains("versionName"));
            var versionMatch = RegexHelpers.ValueRegex.Match(version);
            VersionName = versionMatch.Value;

            var versionCode = packageValues.First(line => line.Contains("versionCode"));
            var versionCodeMatch = RegexHelpers.ValueRegex.Match(versionCode);
            VersionCode = Convert.ToInt32(versionCodeMatch.Value);
            
            // Get Application Labels
            var applicationLabelLine = results.Where(line => line.Contains("application-label")).ToList();
            foreach (var t in applicationLabelLine)
            {
                var appLine = t.Trim().Remove(0, "application-label".Length);
                var values = appLine.Split(':');
                if (string.IsNullOrEmpty(values[0]))
                    values[0] = "culture-neutral";
                var labelMatch = RegexHelpers.ValueRegex.Match(values[1]);
                ApplicationLabels.Add(values[0].TrimStart('-'), labelMatch.Value);
            }
            
            //Permissions uses-permission: name='android.permission.BLUETOOTH'
            var permissionsLines = results.Where(line => line.Contains("uses-permission")).ToList();
            foreach (var p in permissionsLines)
            {
                var permissionMatch = RegexHelpers.ValueRegex.Match(p);
                Permissions.Add(permissionMatch.Value);
            }

            var applicationIconLines = results.Where(line => line.Contains("application-icon")).ToList();
            foreach (var icon in applicationIconLines)
            {
                var iconLine = icon.Trim().Remove(0, "application-icon-".Length);
                var values = iconLine.Split(':');
                var iconMatch = RegexHelpers.ValueRegex.Match(values[1]);
                ApplicationIcons.Add(values[0], iconMatch.Value);
            }
            
            //Application Info
            var applicationLine = results.First(line => line.Contains("application:"));
            {
                var values = applicationLine.Split(' ');
                var labelLine = values.First(line => line.Contains("label="));
                ApplicationLabel = RegexHelpers.ValueRegex.Match(labelLine).Value;
                
                var iconLine = values.First(line => line.Contains("icon="));
                ApplicationIcon = RegexHelpers.ValueRegex.Match(iconLine).Value;
            }
            
            var screensLine = results.First(line => line.Contains("supports-screens:"));
            {
                var values = Regex.Matches(screensLine, RegexHelpers.ValuePattern);
                ScreenSizes = values.Cast<Match>().Select(screenMatch => screenMatch.Value).ToList();
            }
            
            var densitiesLine = results.First(line => line.Contains("densities:"));
            {
                var values = Regex.Matches(densitiesLine, RegexHelpers.ValuePattern);
                Densities = values.Cast<Match>().Select(density => Convert.ToInt32(density.Value)).ToList();
            }
            
            var localesLine = results.First(line => line.Contains("locales:"));
            {
                var values = Regex.Matches(localesLine, RegexHelpers.ValuePattern);
                Locales = values.Cast<Match>().Select(locale => locale.Value).ToList();
            }

            var anyDensityLine = results.First(line => line.Contains("supports-any-density:"));
            SupportsAnyDensity = Convert.ToBoolean(RegexHelpers.ValueRegex.Match(anyDensityLine).Value);
            
            var nativeCodeLine = results.FirstOrDefault(line => line.Contains("native-code:"));
            if (!string.IsNullOrEmpty(nativeCodeLine))
                NativeCode = RegexHelpers.ValueRegex.Match(nativeCodeLine).Value;

            //Feature Group
            var featureGroupLine = results.First(line => line.Contains("feature-group:"));
            {
                AppFeatureGroup = new FeatureGroup {Label = RegexHelpers.NameRegex.Match(featureGroupLine).Value};

                var features = results.Where(line => line.Contains("uses-feature:"));
                foreach (var feature in features)
                {
                    var newFeature = new FeatureGroup.Feature
                    {
                        Name = RegexHelpers.NameRegex.Match(feature).Value, 
                        Reason = RegexHelpers.ReasonRegex.Match(feature).Value
                    };
                    AppFeatureGroup.Features.Add(newFeature);
                }
                
                var impliedFeatures = results.Where(line => line.Contains("uses-implied-feature:"));
                foreach (var feature in impliedFeatures)
                {
                    var newFeature = new FeatureGroup.Feature
                    {
                        Name = RegexHelpers.NameRegex.Match(feature).Value, 
                        Reason = RegexHelpers.ReasonRegex.Match(feature).Value,
                        Implied = true
                    };
                    AppFeatureGroup.Features.Add(newFeature);
                }
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
