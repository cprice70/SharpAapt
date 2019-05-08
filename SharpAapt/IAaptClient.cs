using System;
namespace SharpAapt
{
    public interface IAaptClient
    {
        string AaptPath { get; set; }

        string GetBadgingString(string apkPath);
    }
}
