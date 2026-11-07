using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SharpAapt
{
    public interface IAaptClient
    {
        /// <summary>
        /// Path to the aapt or aapt2 executable. See <see cref="AaptLocator.Find(bool)"/>.
        /// </summary>
        string AaptPath { get; set; }

        /// <summary>
        /// How long a single aapt call may run before it is killed and <see cref="TimeoutException"/> is thrown.
        /// Use <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> for no limit.
        /// </summary>
        TimeSpan Timeout { get; set; }

        ApkBadging GetApkBadging(string apkPath);
        Task<ApkBadging> GetApkBadgingAsync(string apkPath, CancellationToken cancellationToken = default);
        string GetBadgingString(string apkPath);
        Task<string> GetBadgingStringAsync(string apkPath, CancellationToken cancellationToken = default);

        ApkStrings GetApkStrings(string apkPath);
        Task<ApkStrings> GetApkStringsAsync(string apkPath, CancellationToken cancellationToken = default);
        string GetStrings(string apkPath);
        Task<string> GetStringsAsync(string apkPath, CancellationToken cancellationToken = default);

        IEnumerable<string> GetApkPermissions(string apkPath);
        Task<IEnumerable<string>> GetApkPermissionsAsync(string apkPath, CancellationToken cancellationToken = default);
        string GetPermissions(string apkPath);
        Task<string> GetPermissionsAsync(string apkPath, CancellationToken cancellationToken = default);

        IEnumerable<string> GetApkConfigurations(string apkPath);
        Task<IEnumerable<string>> GetApkConfigurationsAsync(string apkPath, CancellationToken cancellationToken = default);
        string GetConfigurations(string apkPath);
        Task<string> GetConfigurationsAsync(string apkPath, CancellationToken cancellationToken = default);

        string GetApkXmlTree(string apkPath, string file = AaptClient.ManifestFile);
        Task<string> GetApkXmlTreeAsync(string apkPath, string file = AaptClient.ManifestFile, CancellationToken cancellationToken = default);
        string GetApkXmlStrings(string apkPath, string file = AaptClient.ManifestFile);
        Task<string> GetApkXmlStringsAsync(string apkPath, string file = AaptClient.ManifestFile, CancellationToken cancellationToken = default);
    }
}
