using System;
using System.Diagnostics;
using System.Text;
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
            if (string.IsNullOrWhiteSpace(AaptPath)) throw new Exception("Aapt path not set. Use AaptClient.Install.AaptPath");
            
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
                    Arguments = $"dump badging {apkPath}"
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

        public string GetStrings(string apkPath)
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
                    Arguments = $"dump strings {apkPath}"
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
