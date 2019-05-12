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

        private static StringBuilder netOutput;

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

        public string GetBadgingString(string apkPath)
        {
            if (string.IsNullOrWhiteSpace(AaptPath))
                throw new Exception("Aapt path not set. Use AaptClient.Install.AaptPath");

            return GetDumpCommand("badging", apkPath);
        }

        public Task<string> GetBadgingStringAsync(string apkPath)
        {
            var tcs1 = new TaskCompletionSource<string>();

            Task.Run(() =>
            {
                var result = GetBadgingString(apkPath);
                tcs1.SetResult(result);
            });
            return tcs1.Task;
        }

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
                throw new Exception("Aapt path not set. Use AaptClient.Install.AaptPath");
            
            netOutput = new StringBuilder();
            var p = new Process
            {
                StartInfo =
                {
                    FileName = AaptPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Arguments = $"dump {command} {apkPath}"
                }
            };
            p.Start();

            p.OutputDataReceived += NetOutputDataHandler;

            p.ErrorDataReceived += NetOutputDataHandler;

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();
            return netOutput.ToString();
        }

        #endregion
        
        private static void NetOutputDataHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            // Collect the net view command output.
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                // Add the text to the collected output.
                netOutput.Append(Environment.NewLine + "  " + outLine.Data);
            }
        }
    }
}
