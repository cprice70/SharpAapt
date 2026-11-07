using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SharpAapt
{
    /// <summary>
    /// Runs <c>aapt dump</c> or <c>aapt2 dump</c> commands against an APK and parses the output.
    /// </summary>
    public class AaptClient : IAaptClient
    {
        /// <summary>
        /// The file inside the APK that the XML dump commands read by default.
        /// </summary>
        public const string ManifestFile = "AndroidManifest.xml";

        private static AaptClient instance;

        private TimeSpan timeout = TimeSpan.FromMinutes(2);

        /// <summary>
        /// Gets or sets the current global instance of the <see cref="AaptClient"/> singleton.
        /// </summary>
        public static AaptClient Instance
        {
            get => instance ?? (instance = new AaptClient());

            set => instance = value;
        }

        /// <inheritdoc />
        public string AaptPath { get; set; } = string.Empty;

        /// <inheritdoc />
        public TimeSpan Timeout
        {
            get => timeout;
            set
            {
                if (value <= TimeSpan.Zero && value != System.Threading.Timeout.InfiniteTimeSpan)
                    throw new ArgumentOutOfRangeException(nameof(value), "Timeout must be positive or Timeout.InfiniteTimeSpan.");
                timeout = value;
            }
        }

        #region Badging

        public ApkBadging GetApkBadging(string apkPath) => ParseBadging(GetBadgingString(apkPath));

        public Task<ApkBadging> GetApkBadgingAsync(string apkPath, CancellationToken cancellationToken = default) =>
            RunAsync(ct => ParseBadging(Dump("badging", apkPath, null, ct)), cancellationToken);

        public string GetBadgingString(string apkPath) => Dump("badging", apkPath, null, CancellationToken.None);

        public Task<string> GetBadgingStringAsync(string apkPath, CancellationToken cancellationToken = default) =>
            RunAsync(ct => Dump("badging", apkPath, null, ct), cancellationToken);

        private static ApkBadging ParseBadging(string output) =>
            string.IsNullOrWhiteSpace(output) ? null : new ApkBadging(output);

        #endregion

        #region Strings

        public ApkStrings GetApkStrings(string apkPath) => ParseStrings(GetStrings(apkPath));

        public Task<ApkStrings> GetApkStringsAsync(string apkPath, CancellationToken cancellationToken = default) =>
            RunAsync(ct => ParseStrings(Dump("strings", apkPath, null, ct)), cancellationToken);

        public string GetStrings(string apkPath) => Dump("strings", apkPath, null, CancellationToken.None);

        public Task<string> GetStringsAsync(string apkPath, CancellationToken cancellationToken = default) =>
            RunAsync(ct => Dump("strings", apkPath, null, ct), cancellationToken);

        private static ApkStrings ParseStrings(string output) =>
            string.IsNullOrWhiteSpace(output) ? null : new ApkStrings(output);

        #endregion

        #region Permissions

        public IEnumerable<string> GetApkPermissions(string apkPath) => ParsePermissions(GetPermissions(apkPath));

        public Task<IEnumerable<string>> GetApkPermissionsAsync(string apkPath, CancellationToken cancellationToken = default) =>
            RunAsync(ct => ParsePermissions(Dump("permissions", apkPath, null, ct)), cancellationToken);

        public string GetPermissions(string apkPath) => Dump("permissions", apkPath, null, CancellationToken.None);

        public Task<string> GetPermissionsAsync(string apkPath, CancellationToken cancellationToken = default) =>
            RunAsync(ct => Dump("permissions", apkPath, null, ct), cancellationToken);

        private static IEnumerable<string> ParsePermissions(string output) =>
            Regex.Matches(output, RegexHelpers.NamePattern).Cast<Match>().Select(m => m.Value).ToList();

        #endregion

        #region Configurations

        public IEnumerable<string> GetApkConfigurations(string apkPath) => ParseConfigurations(GetConfigurations(apkPath));

        public Task<IEnumerable<string>> GetApkConfigurationsAsync(string apkPath, CancellationToken cancellationToken = default) =>
            RunAsync(ct => ParseConfigurations(Dump("configurations", apkPath, null, ct)), cancellationToken);

        public string GetConfigurations(string apkPath) => Dump("configurations", apkPath, null, CancellationToken.None);

        public Task<string> GetConfigurationsAsync(string apkPath, CancellationToken cancellationToken = default) =>
            RunAsync(ct => Dump("configurations", apkPath, null, ct), cancellationToken);

        private static IEnumerable<string> ParseConfigurations(string output) =>
            output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .ToList();

        #endregion

        #region Xml

        /// <summary>
        /// Dumps the compiled XML tree of <paramref name="file"/> inside the APK (the manifest by default).
        /// </summary>
        public string GetApkXmlTree(string apkPath, string file = ManifestFile) =>
            Dump("xmltree", apkPath, file, CancellationToken.None);

        public Task<string> GetApkXmlTreeAsync(string apkPath, string file = ManifestFile, CancellationToken cancellationToken = default) =>
            RunAsync(ct => Dump("xmltree", apkPath, file, ct), cancellationToken);

        /// <summary>
        /// Dumps the string pool of <paramref name="file"/> inside the APK (the manifest by default).
        /// </summary>
        public string GetApkXmlStrings(string apkPath, string file = ManifestFile) =>
            Dump("xmlstrings", apkPath, file, CancellationToken.None);

        public Task<string> GetApkXmlStringsAsync(string apkPath, string file = ManifestFile, CancellationToken cancellationToken = default) =>
            RunAsync(ct => Dump("xmlstrings", apkPath, file, ct), cancellationToken);

        #endregion

        #region Dump Command

        private static Task<T> RunAsync<T>(Func<CancellationToken, T> work, CancellationToken cancellationToken) =>
            Task.Run(() => work(cancellationToken), cancellationToken);

        private string Dump(string command, string apkPath, string file, CancellationToken cancellationToken)
        {
            var aaptPath = AaptPath;
            if (string.IsNullOrWhiteSpace(aaptPath))
                throw new InvalidOperationException(
                    "Aapt path not set. Set AaptClient.Instance.AaptPath, for example to AaptLocator.Find().");

            cancellationToken.ThrowIfCancellationRequested();

            // Buffers are per call so concurrent calls never share output.
            var output = new StringBuilder();
            var error = new StringBuilder();

            using (var p = new Process
            {
                StartInfo =
                {
                    FileName = aaptPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Arguments = BuildArguments(command, apkPath, file, IsAapt2(aaptPath))
                }
            })
            {
                p.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) output.AppendLine(e.Data);
                };
                p.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) error.AppendLine(e.Data);
                };

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                bool exited;
                using (cancellationToken.Register(() => TryKill(p)))
                {
                    exited = WaitForExit(p, timeout);
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    TryKill(p);
                    throw new OperationCanceledException(cancellationToken);
                }

                if (!exited)
                {
                    TryKill(p);
                    throw new TimeoutException($"aapt dump {command} did not finish within {timeout}.");
                }

                // Lets the async output handlers drain after a timed wait.
                p.WaitForExit();

                if (p.ExitCode != 0)
                {
                    var stderr = error.ToString().Trim();
                    throw new AaptException(
                        $"aapt dump {command} failed with exit code {p.ExitCode}: {stderr}",
                        p.ExitCode,
                        stderr);
                }

                return output.ToString();
            }
        }

        internal static bool IsAapt2(string aaptPath) =>
            string.Equals(Path.GetFileNameWithoutExtension(aaptPath), "aapt2", StringComparison.OrdinalIgnoreCase);

        // aapt:  dump <command> <apk> [file]
        // aapt2: dump <command> [--file <file>] <apk>
        private static string BuildArguments(string command, string apkPath, string file, bool aapt2)
        {
            if (file == null)
                return $"dump {command} {QuoteArgument(apkPath)}";

            return aapt2
                ? $"dump {command} --file {QuoteArgument(file)} {QuoteArgument(apkPath)}"
                : $"dump {command} {QuoteArgument(apkPath)} {QuoteArgument(file)}";
        }

        private static bool WaitForExit(Process process, TimeSpan timeout)
        {
            if (timeout == System.Threading.Timeout.InfiniteTimeSpan)
            {
                process.WaitForExit();
                return true;
            }

            return process.WaitForExit((int)Math.Min(timeout.TotalMilliseconds, int.MaxValue));
        }

        private static void TryKill(Process process)
        {
            try
            {
                if (!process.HasExited) process.Kill();
            }
            catch (InvalidOperationException)
            {
                // Already exited.
            }
            catch (Win32Exception)
            {
                // Exiting or access denied; nothing more to do.
            }
        }

        private static string QuoteArgument(string argument)
        {
            if (string.IsNullOrEmpty(argument)) return "\"\"";
            return "\"" + argument.Replace("\"", "\\\"") + "\"";
        }

        #endregion
    }
}
