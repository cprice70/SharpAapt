using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpAapt;

namespace SharpAapt.Tests
{
    /// <summary>
    /// Runs the real aapt and aapt2 from the local Android SDK against Fixtures/test.apk,
    /// which is built from TestApp/ by TestApp/build.sh. Skipped when the tool isn't installed.
    /// </summary>
    [Category("Integration")]
    [TestFixture("aapt")]
    [TestFixture("aapt2")]
    public class IntegrationTests
    {
        private readonly string tool;
        private AaptClient client;
        private string apk;

        public IntegrationTests(string tool)
        {
            this.tool = tool;
        }

        [SetUp]
        public void Setup()
        {
            var path = AaptLocator.FindTool(AaptLocator.DefaultSdkRoots(), tool);
            if (path == null)
                Assert.Ignore($"{tool} not found. Set ANDROID_HOME to an SDK with build-tools installed.");

            client = new AaptClient { AaptPath = path };
            apk = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "test.apk");
        }

        [Test]
        public void Badging()
        {
            var badging = client.GetApkBadging(apk);

            Assert.That(badging.PackageName, Is.EqualTo("com.example.sharpaapt"));
            Assert.That(badging.VersionCode, Is.EqualTo(42));
            Assert.That(badging.VersionName, Is.EqualTo("1.0 beta"));
            Assert.That(badging.SdkVersionLevel, Is.EqualTo(21));
            Assert.That(badging.TargetSdkVersionLevel, Is.EqualTo(34));
            Assert.That(badging.InstallLocation, Is.EqualTo("auto"));
            Assert.That(badging.ApplicationLabel, Is.EqualTo("My App"));
            Assert.That(badging.ApplicationLabels["de"], Is.EqualTo("Meine App: Deluxe"));
            Assert.That(badging.Permissions, Is.EqualTo(new[] { "android.permission.INTERNET", "android.permission.BLUETOOTH" }));
            Assert.That(badging.Locales, Does.Contain("de"));
            Assert.That(badging.NativeCode, Is.Empty);
        }

        [Test]
        public async Task BadgingAsync()
        {
            var badging = await client.GetApkBadgingAsync(apk);

            Assert.That(badging.PackageName, Is.EqualTo("com.example.sharpaapt"));
        }

        [Test]
        public void Permissions()
        {
            var permissions = client.GetApkPermissions(apk);

            Assert.That(permissions, Is.EqualTo(new[] { "android.permission.INTERNET", "android.permission.BLUETOOTH" }));
        }

        [Test]
        public void Strings()
        {
            var strings = client.GetApkStrings(apk);

            Assert.That(strings.Strings.Values, Does.Contain("https://example.com/path"));
            Assert.That(strings.Strings.Values, Does.Contain("Meine App: Deluxe"));
        }

        [Test]
        public void Configurations()
        {
            var configs = client.GetApkConfigurations(apk).ToList();

            Assert.That(configs, Does.Contain("de"));
        }

        [Test]
        public void XmlTree()
        {
            var tree = client.GetApkXmlTree(apk);

            Assert.That(tree, Does.Contain("E: manifest"));
        }

        [Test]
        public void MissingApkThrowsAaptException()
        {
            Assert.Throws<AaptException>(() => client.GetBadgingString(apk + ".missing"));
        }
    }
}
