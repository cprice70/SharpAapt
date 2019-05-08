using System;

namespace SharpAapt.Demo
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var apkPath = "/Users/$username%/Downloads/test.apk";

            AaptClient.Instance.AaptPath = "PathToAapt";

            var result = AaptClient.Instance.GetBadgingString(apkPath);

            Console.WriteLine("**** Apk Badging ****");
            Console.WriteLine(result);

            var result2 = await AaptClient.Instance.GetBadgingStringAsync(apkPath);

            Console.WriteLine("**** Apk Badging Async****");
            Console.WriteLine(result2);

            var badging = AaptClient.Instance.GetBadging(apkPath);
        }
    }
}
