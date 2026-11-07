using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SharpAapt
{
    public class AaptClient : IAaptClient
    {
        /// <summary>
        /// The singleton instance of the <see cref="AaptClient"/> class.
        /// </summary>
        private static AaptClient instance;

        /// <summary>
        /// Gets or sets the current global instance of the <see cref="AaptClient"/> singleton.
        /// </summary>
        public static AaptClient Instance
        {
            get => instance ?? (instance = new AaptClient());

            set => instance = value;
        }

        public string AaptPath { get; set; } = string.Empty;

        #region Badging

        public ApkBadging GetApkBadging(string apkPath)
        {
            var badging = GetBadgingString(apkPath);

            if (string.IsNullOrWhiteSpace(badging)) return null;

            var apkBadging = new ApkBadging(badging);

            return apkBadging;
        }

        public string GetBadgingString(string apkPath) => GetDumpCommand("badging", apkPath);

        public Task<string> GetBadgingStringAsync(string apkPath) => Task.Run(() => GetBadgingString(apkPath));

        #endregion

        #region Strings

        public ApkStrings GetApkStrings(string apkPath)
        {
            var strings = GetStrings(apkPath);

            if (string.IsNullOrWhiteSpace(strings)) return null;

            var apkStrings = new ApkStrings(strings);

            return apkStrings;
        }

        public string GetStrings(string apkPath) => GetDumpCommand("strings", apkPath);

        #endregion

        #region Permissions

        public string GetPermissions(string apkPath) => GetDumpCommand("permissions", apkPath);

        public IEnumerable<string> GetApkPermissions(string apkPath)
        {
            var output = GetPermissions(apkPath);

            var values = Regex.Matches(output, RegexHelpers.NamePattern);
            return values.Cast<Match>().Select(permission => permission.Value).ToList();
        }

        #endregion

        #region Configurations

        public string GetConfigurations(string apkPath)
        {
            return GetDumpCommand("configurations", apkPath);
        }

        public IEnumerable<string> GetApkConfigurations(string apkPath)
        {
            var configs = GetConfigurations(apkPath);
            //Split string into list, remove whitespace and any empty strings
            return configs.Split('\n').Select(c => c.Trim()).ToList().Where(line => !string.IsNullOrEmpty(line));
        }

        #endregion

        #region Xml
        public string GetApkXmlTree(string apkPath) => GetDumpCommand("xmltree", apkPath);

        public string GetApkXmlStrings(string apkPath) => GetDumpCommand("xmlstrings", apkPath);
        
        #endregion
        
        #region Dump Command
        private string GetDumpCommand(string command, string apkPath)
        {
            if (string.IsNullOrWhiteSpace(AaptPath))
                throw new InvalidOperationException("Aapt path not set. Use AaptClient.Instance.AaptPath");

            // Buffers are per call so concurrent calls never share output.
            var output = new StringBuilder();
            var error = new StringBuilder();

            using (var p = new Process
            {
                StartInfo =
                {
                    FileName = AaptPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Arguments = $"dump {command} {QuoteArgument(apkPath)}"
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

        private static string QuoteArgument(string argument)
        {
            if (string.IsNullOrEmpty(argument)) return "\"\"";
            return "\"" + argument.Replace("\"", "\\\"") + "\"";
        }

        #endregion
    }
}
