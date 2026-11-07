using System.IO;
using NUnit.Framework;
using SharpAapt;

namespace SharpAapt.Tests
{
    /// <summary>
    /// Parses Fixtures/test.apk with both aapt and aapt2 and checks every result matches,
    /// so callers get the same answers whichever tool <see cref="AaptLocator"/> picks.
    /// Skipped unless both tools are installed.
    /// </summary>
    [Category("Integration")]
    public class ToolParityTests
    {
        private AaptClient aapt;
        private AaptClient aapt2;
        private string apk;

        [OneTimeSetUp]
        public void Setup()
        {
            var roots = AaptLocator.DefaultSdkRoots();
            var aaptPath = AaptLocator.FindTool(roots, "aapt");
            var aapt2Path = AaptLocator.FindTool(roots, "aapt2");
            if (aaptPath == null || aapt2Path == null)
                RequireSdk.Missing("Both aapt and aapt2 are needed. Set ANDROID_HOME to an SDK with build-tools installed.");

            aapt = new AaptClient { AaptPath = aaptPath };
            aapt2 = new AaptClient { AaptPath = aapt2Path };
            apk = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "test.apk");
        }

        [Test]
        public void BadgingMatches()
        {
            var a = aapt.GetApkBadging(apk);
            var b = aapt2.GetApkBadging(apk);

            Assert.Multiple(() =>
            {
                Assert.That(b.PackageName, Is.EqualTo(a.PackageName), nameof(a.PackageName));
                Assert.That(b.VersionName, Is.EqualTo(a.VersionName), nameof(a.VersionName));
                Assert.That(b.VersionCode, Is.EqualTo(a.VersionCode), nameof(a.VersionCode));
                Assert.That(b.SdkVersionLevel, Is.EqualTo(a.SdkVersionLevel), nameof(a.SdkVersionLevel));
                Assert.That(b.TargetSdkVersionLevel, Is.EqualTo(a.TargetSdkVersionLevel), nameof(a.TargetSdkVersionLevel));
                Assert.That(b.InstallLocation, Is.EqualTo(a.InstallLocation), nameof(a.InstallLocation));
                Assert.That(b.ApplicationLabel, Is.EqualTo(a.ApplicationLabel), nameof(a.ApplicationLabel));
                Assert.That(b.ApplicationIcon, Is.EqualTo(a.ApplicationIcon), nameof(a.ApplicationIcon));
                Assert.That(b.ApplicationLabels, Is.EquivalentTo(a.ApplicationLabels), nameof(a.ApplicationLabels));
                Assert.That(b.ApplicationIcons, Is.EquivalentTo(a.ApplicationIcons), nameof(a.ApplicationIcons));
                Assert.That(b.Permissions, Is.EqualTo(a.Permissions), nameof(a.Permissions));
                Assert.That(b.ScreenSizes, Is.EqualTo(a.ScreenSizes), nameof(a.ScreenSizes));
                Assert.That(b.Densities, Is.EqualTo(a.Densities), nameof(a.Densities));
                Assert.That(b.Locales, Is.EqualTo(a.Locales), nameof(a.Locales));
                Assert.That(b.SupportsAnyDensity, Is.EqualTo(a.SupportsAnyDensity), nameof(a.SupportsAnyDensity));
                Assert.That(b.NativeCode, Is.EqualTo(a.NativeCode), nameof(a.NativeCode));
                Assert.That(b.AppFeatureGroup.Label, Is.EqualTo(a.AppFeatureGroup.Label), "FeatureGroup.Label");
                Assert.That(b.AppFeatureGroup.Features, Is.EquivalentTo(a.AppFeatureGroup.Features), "FeatureGroup.Features");
            });
        }

        [Test]
        public void PermissionsMatch()
        {
            Assert.That(aapt2.GetApkPermissions(apk), Is.EqualTo(aapt.GetApkPermissions(apk)));
        }

        [Test]
        public void StringsMatch()
        {
            // aapt and aapt2 print the string pool in different orders, so the "String #N" keys
            // differ between tools. Only the set of values is the same.
            Assert.That(aapt2.GetApkStrings(apk).Strings.Values, Is.EquivalentTo(aapt.GetApkStrings(apk).Strings.Values));
        }

        [Test]
        public void ConfigurationsMatch()
        {
            Assert.That(aapt2.GetApkConfigurations(apk), Is.EquivalentTo(aapt.GetApkConfigurations(apk)));
        }
    }
}
