using System;
using System.Linq;
using NUnit.Framework;
using SharpAapt;

namespace Tests
{
    public class Tests
    {
        private const string TestApk = @"../../../com.google.android.gm_2019.04.14.246198419.release-61418844_minAPI19(nodpi).apk";

        [SetUp]
        public void Setup()
        {
            var home = Environment.GetEnvironmentVariable("HOME");
            AaptClient.Instance.AaptPath = $"{home}/Library/Developer/Xamarin/android-sdk-macosx/build-tools/28.0.3/aapt";
        }

        [Test]
        public void AaptPermissionsTest()
        {
            var permissions = AaptClient.Instance.GetApkPermissions(TestApk);
            Assert.True(permissions.Any());
        }

        [Test]
        public void AaptApplicationLabelTest()
        {
            var badging = AaptClient.Instance.GetApkBadging(TestApk);
            
            Assert.True(!string.IsNullOrEmpty(badging.ApplicationLabel));
        }

        [Test]
        public void AaptScreenSizesTest()
        {
            var screens = AaptClient.Instance.GetApkBadging(TestApk);
            
            Assert.True(screens.ScreenSizes.Any());
        }

        [Test]
        public void AaptConfigurationsTest()
        {
            var configs = AaptClient.Instance.GetApkConfigurations(TestApk);
            
            Assert.True(configs.Any());
        }

        [Test]
        public void AaptStringsTest()
        {
            var strings = AaptClient.Instance.GetApkStrings(TestApk);
            
            Assert.True(strings.Strings.Any());
        }
    }
}