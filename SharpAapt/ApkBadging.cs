using System;
using System.Linq;

namespace SharpAapt
{
    public class ApkBadging
    {
        public ApkBadging(string apkBadgingString)
        {
            var results = apkBadgingString.Split('\n');
            var lines = results.First(line => line.Contains("sdkVersion:"));
        }

        public int SdkVersionLevel { get; }

        public int TargetSdkVersionLevel { get; }
    }
}
