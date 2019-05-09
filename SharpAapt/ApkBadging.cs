using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SharpAapt
{
    public class ApkBadging
    {
        public ApkBadging(string apkBadgingString)
        {
            Regex value_regex = new Regex(@"(?<=\')(.*?)(?=\')");

            var results = apkBadgingString.Split('\n');

            // Get SdkVersion
            var vline = results.First(line => line.Contains("sdkVersion:"));
            Regex regex = new Regex("\\d+");
            var match = regex.Match(vline);
            SdkVersionLevel = Convert.ToInt32(match.Value);

            // Get SdkVersion
            var tline = results.First(line => line.Contains("targetSdkVersion:"));
            match = regex.Match(tline);
            TargetSdkVersionLevel = Convert.ToInt32(match.Value);

            // Get InstallLocation
            var installLine = results.First(line => line.Contains("install-location:"));
            var install_match = value_regex.Match(installLine);
            InstallLocation = install_match.Value;

            // Get InstallLocation
            var packageline = results.First(line => line.Contains("package:"));
            var packagelineValue = packageline.Split(':');
            var packageValues = packagelineValue[1].Split(' ');

            var packageName = packageValues.First(line => line.Contains("name"));
            var packageName_match = value_regex.Match(packageName);
            PackageName = packageName_match.Value;

            var version = packageValues.First(line => line.Contains("versionName"));
            var version_match = value_regex.Match(version);
            VersionName = new Version(version_match.Value);

            var versionCode = packageValues.First(line => line.Contains("versionCode"));
            var versionCode_match = value_regex.Match(versionCode);
            VersionCode = Convert.ToInt32(versionCode_match.Value);
        }

        public int SdkVersionLevel { get; }

        public int TargetSdkVersionLevel { get; }

        public string InstallLocation { get; }

        public string PackageName { get; }

        public Version VersionName { get; }

        public int VersionCode { get; }
    }
}
