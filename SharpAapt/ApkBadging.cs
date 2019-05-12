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
            const string valuePattern = @"(?<=\')(\S+)(?=\')";
            var valueRegex = new Regex(valuePattern);
            const string namePattern = @"(?<=name\=\')(.*?)(?=\')";
            var nameRegex = new Regex(namePattern);
            const string reasonPattern = @"(?<=reason\=\')(.*?)(?=\')";
            var reasonRegex = new Regex(reasonPattern);
            
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
            var installLine = results.First(line => line.Contains("install-location:"));
            var installMatch = valueRegex.Match(installLine);
            InstallLocation = installMatch.Value;

            // Get Package Info
            var packageLine = results.First(line => line.Contains("package:"));
            var packageLineValue = packageLine.Split(':');
            var packageValues = packageLineValue[1].Split(' ');

            var packageName = packageValues.First(line => line.Contains("name"));
            var packageNameMatch = valueRegex.Match(packageName);
            PackageName = packageNameMatch.Value;

            var version = packageValues.First(line => line.Contains("versionName"));
            var versionMatch = valueRegex.Match(version);
            VersionName = new Version(versionMatch.Value);

            var versionCode = packageValues.First(line => line.Contains("versionCode"));
            var versionCodeMatch = valueRegex.Match(versionCode);
            VersionCode = Convert.ToInt32(versionCodeMatch.Value);
            
            // Get Application Labels
            var applicationLabelLine = results.Where(line => line.Contains("application-label")).ToList();
            foreach (var t in applicationLabelLine)
            {
                var appLine = t.Trim().Remove(0, "application-label".Length);
                var values = appLine.Split(':');
                if (string.IsNullOrEmpty(values[0]))
                    values[0] = "culture-neutral";
                var labelMatch = valueRegex.Match(values[1]);
                ApplicationLabels.Add(values[0].TrimStart('-'), labelMatch.Value);
            }
            
            //Permissions uses-permission: name='android.permission.BLUETOOTH'
            var permissionsLines = results.Where(line => line.Contains("uses-permission")).ToList();
            foreach (var p in permissionsLines)
            {
                var permissionMatch = valueRegex.Match(p);
                Permissions.Add(permissionMatch.Value);
            }

            var applicationIconLines = results.Where(line => line.Contains("application-icon")).ToList();
            foreach (var icon in applicationIconLines)
            {
                var iconLine = icon.Trim().Remove(0, "application-icon-".Length);
                var values = iconLine.Split(':');
                var iconMatch = valueRegex.Match(values[1]);
                ApplicationIcons.Add(values[0], iconMatch.Value);
            }
            
            //Application Info
            var applicationLine = results.First(line => line.Contains("application:"));
            {
                var values = applicationLine.Split(' ');
                var labelLine = values.First(line => line.Contains("label="));
                ApplicationLabel = valueRegex.Match(labelLine).Value;
                
                var iconLine = values.First(line => line.Contains("icon="));
                ApplicationIcon = valueRegex.Match(iconLine).Value;
            }
            
            var screensLine = results.First(line => line.Contains("supports-screens:"));
            {
                var values = Regex.Matches(screensLine, valuePattern);
                ScreenSizes = values.Cast<Match>().Select(screenMatch => screenMatch.Value).ToList();
            }
            
            var densitiesLine = results.First(line => line.Contains("densities:"));
            {
                var values = Regex.Matches(densitiesLine, valuePattern);
                Densities = values.Cast<Match>().Select(density => Convert.ToInt32(density.Value)).ToList();
            }
            
            var localesLine = results.First(line => line.Contains("locales:"));
            {
                var values = Regex.Matches(localesLine, valuePattern);
                Locales = values.Cast<Match>().Select(locale => locale.Value).ToList();
            }

            var anyDensityLine = results.First(line => line.Contains("supports-any-density:"));
            SupportsAnyDensity = Convert.ToBoolean(valueRegex.Match(anyDensityLine).Value);
            
            var nativeCodeLine = results.First(line => line.Contains("native-code:"));
            NativeCode = valueRegex.Match(nativeCodeLine).Value;

            //Feature Group
            var featureGroupLine = results.First(line => line.Contains("feature-group:"));
            {
                AppFeatureGroup = new FeatureGroup {Label = nameRegex.Match(featureGroupLine).Value};

                var features = results.Where(line => line.Contains("uses-feature:"));
                foreach (var feature in features)
                {
                    var newFeature = new FeatureGroup.Feature
                    {
                        Name = nameRegex.Match(feature).Value, 
                        Reason = reasonRegex.Match(feature).Value
                    };
                    AppFeatureGroup.Features.Add(newFeature);
                }
                
                var impliedFeatures = results.Where(line => line.Contains("uses-implied-feature:"));
                foreach (var feature in impliedFeatures)
                {
                    var newFeature = new FeatureGroup.Feature
                    {
                        Name = nameRegex.Match(feature).Value, 
                        Reason = reasonRegex.Match(feature).Value,
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

        public Version VersionName { get; }

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
