using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpAapt;

namespace SharpAapt.Tests
{
    /// <summary>
    /// Runs <see cref="AaptClient"/> against a fake aapt shell script, so no Android SDK is needed.
    /// </summary>
    public class AaptClientTests
    {
        private const string FakeAapt =
            "#!/bin/sh\n" +
            "echo run >> \"$(dirname \"$0\")/runs.log\"\n" +
            "case \"$3\" in *fail*) echo 'ERROR: dump failed' >&2; exit 1;; esac\n" +
            "printf 'arg:%s\\n' \"$@\"\n";

        private string workDir;
        private AaptClient client;

        [SetUp]
        public void Setup()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Assert.Ignore("Fake aapt is a POSIX shell script.");

            workDir = Path.Combine(Path.GetTempPath(), "sharpaapt-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(workDir);
            var script = Path.Combine(workDir, "aapt");
            File.WriteAllText(script, FakeAapt);
            Process.Start("chmod", $"+x \"{script}\"").WaitForExit();

            client = new AaptClient { AaptPath = script };
        }

        [TearDown]
        public void TearDown()
        {
            if (workDir != null && Directory.Exists(workDir))
                Directory.Delete(workDir, true);
        }

        private string[] Lines(string output) =>
            output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).ToArray();

        [Test]
        public void RunsAaptOncePerCall()
        {
            client.GetStrings("/tmp/app.apk");

            var runs = File.ReadAllLines(Path.Combine(workDir, "runs.log"));
            Assert.That(runs.Length, Is.EqualTo(1));
        }

        [Test]
        public void PassesPathWithSpacesAsOneArgument()
        {
            var output = client.GetStrings("/tmp/my app.apk");

            Assert.That(Lines(output), Is.EqualTo(new[] { "arg:dump", "arg:strings", "arg:/tmp/my app.apk" }));
        }

        [Test]
        public void ThrowsAaptExceptionOnFailure()
        {
            var ex = Assert.Throws<AaptException>(() => client.GetStrings("/tmp/fail.apk"));
            Assert.That(ex.ExitCode, Is.EqualTo(1));
            Assert.That(ex.Message, Does.Contain("ERROR: dump failed"));
        }

        [Test]
        public void ConcurrentCallsDoNotMixOutput()
        {
            var paths = Enumerable.Range(0, 20).Select(i => $"/tmp/app{i}.apk").ToArray();

            var outputs = paths.AsParallel().WithDegreeOfParallelism(8)
                .Select(p => (path: p, output: client.GetStrings(p)))
                .ToArray();

            foreach (var (path, output) in outputs)
                Assert.That(Lines(output), Is.EqualTo(new[] { "arg:dump", "arg:strings", "arg:" + path }));
        }

        [Test]
        public void AsyncFaultsInsteadOfHanging()
        {
            var task = client.GetBadgingStringAsync("/tmp/fail.apk");

            var finished = Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(10))).Result == task;
            Assert.That(finished, Is.True, "Task never completed");
            Assert.That(task.Exception?.InnerException, Is.TypeOf<AaptException>());
        }

        [Test]
        public void MissingAaptPathThrowsInvalidOperation()
        {
            var empty = new AaptClient();
            var ex = Assert.Throws<InvalidOperationException>(() => empty.GetStrings("/tmp/app.apk"));
            Assert.That(ex.Message, Does.Contain("AaptClient.Instance.AaptPath"));
        }
    }
}
