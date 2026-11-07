using System.IO;
using System.Linq;
using NUnit.Framework;
using SharpAapt;

namespace SharpAapt.Tests
{
    public class ApkBadgingTests
    {
        private ApkBadging badging;

        [SetUp]
        public void Setup()
        {
            var text = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "badging.txt"));
            badging = new ApkBadging(text);
        }

        [Test]
        public void ParsesPackageInfo()
        {
            Assert.That(badging.PackageName, Is.EqualTo("com.example.app"));
            Assert.That(badging.VersionCode, Is.EqualTo(42));
            Assert.That(badging.VersionName, Is.EqualTo("1.0 beta"));
        }

        [Test]
        public void ParsesSdkVersions()
        {
            Assert.That(badging.SdkVersionLevel, Is.EqualTo(19));
            Assert.That(badging.TargetSdkVersionLevel, Is.EqualTo(28));
            Assert.That(badging.InstallLocation, Is.EqualTo("auto"));
        }

        [Test]
        public void ParsesLabelsWithSpacesAndColons()
        {
            Assert.That(badging.ApplicationLabel, Is.EqualTo("My App"));
            Assert.That(badging.ApplicationLabels["culture-neutral"], Is.EqualTo("My App"));
            Assert.That(badging.ApplicationLabels["de"], Is.EqualTo("Meine App: Deluxe"));
        }

        [Test]
        public void ParsesIcons()
        {
            Assert.That(badging.ApplicationIcon, Is.EqualTo("res/mipmap-mdpi/ic_launcher.png"));
            Assert.That(badging.ApplicationIcons["240"], Is.EqualTo("res/mipmap-hdpi/ic_launcher.png"));
        }

        [Test]
        public void ParsesPermissions()
        {
            Assert.That(badging.Permissions, Is.EqualTo(new[] { "android.permission.INTERNET", "android.permission.BLUETOOTH" }));
        }

        [Test]
        public void ParsesMultiValueLines()
        {
            Assert.That(badging.ScreenSizes, Is.EqualTo(new[] { "small", "normal", "large", "xlarge" }));
            Assert.That(badging.Densities, Is.EqualTo(new[] { 160, 240, 65535 }));
            Assert.That(badging.Locales, Is.EqualTo(new[] { "--_--", "de" }));
            Assert.That(badging.SupportsAnyDensity, Is.True);
        }

        [Test]
        public void ParsesFeatures()
        {
            var features = badging.AppFeatureGroup.Features;
            Assert.That(features.Count, Is.EqualTo(2));
            Assert.That(features.Single(f => !f.Implied).Name, Is.EqualTo("android.hardware.bluetooth"));
            var implied = features.Single(f => f.Implied);
            Assert.That(implied.Name, Is.EqualTo("android.hardware.touchscreen"));
            Assert.That(implied.Reason, Is.EqualTo("default feature for all apps"));
        }

        [Test]
        public void HandlesWindowsLineEndings()
        {
            var text = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "badging.txt"))
                .Replace("\r\n", "\n").Replace("\n", "\r\n");
            var crlf = new ApkBadging(text);
            Assert.That(crlf.VersionName, Is.EqualTo("1.0 beta"));
            Assert.That(crlf.ApplicationLabel, Is.EqualTo("My App"));
        }
    }

    public class ApkStringsTests
    {
        [Test]
        public void KeepsColonsInValues()
        {
            var text = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "strings.txt"));
            var strings = new ApkStrings(text).Strings;

            Assert.That(strings["String #0"], Is.EqualTo("https://example.com/path"));
            Assert.That(strings["String #1"], Is.EqualTo("Hello"));
            Assert.That(strings["String #2"], Is.EqualTo("a:b:c"));
        }
    }
}
