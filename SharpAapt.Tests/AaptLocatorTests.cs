using System;
using System.IO;
using System.Runtime.InteropServices;
using NUnit.Framework;
using SharpAapt;

namespace SharpAapt.Tests
{
    public class AaptLocatorTests
    {
        private string sdk;

        private static string Exe(string name) =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? name + ".exe" : name;

        [SetUp]
        public void Setup()
        {
            sdk = Path.Combine(Path.GetTempPath(), "sharpaapt-sdk-" + Guid.NewGuid().ToString("N"));
            Tool("34.0.0", "aapt");
            Tool("34.0.0", "aapt2");
            Tool("35.0.0", "aapt2");
            Tool("36.0.0-rc1", "aapt");
            Directory.CreateDirectory(Path.Combine(sdk, "build-tools", "not-a-version"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(sdk)) Directory.Delete(sdk, true);
        }

        private string Tool(string version, string name)
        {
            var dir = Path.Combine(sdk, "build-tools", version);
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, Exe(name));
            File.WriteAllText(path, string.Empty);
            return path;
        }

        [Test]
        public void PrefersNewestAapt2()
        {
            var found = AaptLocator.Find(new[] { sdk }, preferAapt2: true);

            Assert.That(found, Is.EqualTo(Path.Combine(sdk, "build-tools", "35.0.0", Exe("aapt2"))));
        }

        [Test]
        public void CanPreferAapt()
        {
            var found = AaptLocator.Find(new[] { sdk }, preferAapt2: false);

            Assert.That(found, Is.EqualTo(Path.Combine(sdk, "build-tools", "36.0.0-rc1", Exe("aapt"))));
        }

        [Test]
        public void FallsBackToOtherToolWhenPreferredIsMissing()
        {
            File.Delete(Path.Combine(sdk, "build-tools", "34.0.0", Exe("aapt2")));
            File.Delete(Path.Combine(sdk, "build-tools", "35.0.0", Exe("aapt2")));

            var found = AaptLocator.Find(new[] { sdk }, preferAapt2: true);

            Assert.That(found, Is.EqualTo(Path.Combine(sdk, "build-tools", "36.0.0-rc1", Exe("aapt"))));
        }

        [Test]
        public void SkipsMissingAndEmptyRoots()
        {
            var found = AaptLocator.Find(new[] { null, "", Path.Combine(sdk, "missing"), sdk }, preferAapt2: true);

            Assert.That(found, Does.EndWith(Exe("aapt2")));
        }

        [Test]
        public void ReturnsNullWhenNothingFound()
        {
            Assert.That(AaptLocator.Find(new[] { Path.Combine(sdk, "missing") }, preferAapt2: true), Is.Null);
        }
    }
}
