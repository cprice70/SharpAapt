using System;

namespace SharpAapt.Demo
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var apkPath = "/Users/cprice/Downloads/com.lci1.one.apk";

            AaptClient.Instance.AaptPath = "/Users/cprice/Library/Developer/Xamarin/android-sdk-macosx/build-tools/28.0.3/aapt";

            var result = AaptClient.Instance.GetBadgingString(apkPath);

            Console.WriteLine("**** Apk Badging ****");
            Console.WriteLine(result);

            var result2 = await AaptClient.Instance.GetBadgingStringAsync(apkPath);

            Console.WriteLine("**** Apk Badging Async****");
            Console.WriteLine(result2);

            var badging = AaptClient.Instance.GetApkBadging(apkPath);
            
            var strings = AaptClient.Instance.GetApkStrings(apkPath);

            var permissions = AaptClient.Instance.GetApkPermissions(apkPath);

            var configurations = AaptClient.Instance.GetApkConfigurations(apkPath);
        }
    }
}
