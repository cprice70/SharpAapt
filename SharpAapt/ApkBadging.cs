using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SharpAapt
{
    public class ApkBadging
    {
        public ApkBadging(string apkBadgingString)
        {
            var valueRegex = new Regex(@"(?<=\')(.*?)(?=\')");

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
                
                ApplicationLabels.Add(values[0].TrimStart('-'), values[1]);
            }
            
            //Permissions uses-permission: name='android.permission.BLUETOOTH'
            var permissionsLines = results.Where(line => line.Contains("uses-permission")).ToList();
            foreach (var p in permissionsLines)
            {
                var permissionMatch = valueRegex.Match(p);
                Permissions.Add(permissionMatch.Value);
            }
        }

        public int SdkVersionLevel { get; }

        public int TargetSdkVersionLevel { get; }

        public string InstallLocation { get; }

        public string PackageName { get; }

        public Version VersionName { get; }

        public int VersionCode { get; }
        
        public Dictionary<string, string> ApplicationLabels { get; } = new Dictionary<string, string>();
        
        public List<string> Permissions { get; } = new List<string>();
    
    }
}
