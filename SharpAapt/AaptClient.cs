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
        private static AaptClient instance = null;

        private static StringBuilder netOutput;

        /// <summary>
        /// Gets or sets the current global instance of the <see cref="AaptClient"/> singleton.
        /// </summary>
        public static AaptClient Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AaptClient();
                }

                return instance;
            }

            set
            {
                instance = value;
            }
        }

        public string AaptPath { get; set; } = string.Empty;

        public ApkBadging GetBadging(string apkPath)
        {
            var badging = GetBadgingString(apkPath);

            if (string.IsNullOrWhiteSpace(badging)) return null;

            var apkBadging = new ApkBadging(badging);

            return null;
        }

        public string GetBadgingString(string apkPath)
        {
            if (string.IsNullOrWhiteSpace(AaptPath)) throw new Exception("Aapt path not set. Use AaptClient.Install.AaptPath");
            
            netOutput = new StringBuilder();
            var p = new Process();
            p.StartInfo.FileName = AaptPath;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = $"dump badging {apkPath}";
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

        private static void NetOutputDataHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            // Collect the net view command output.
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                // Add the text to the collected output.
                netOutput.Append(Environment.NewLine + "  " + outLine.Data);
            }
        }
    }
}
