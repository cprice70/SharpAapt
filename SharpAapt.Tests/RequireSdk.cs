using System;
using NUnit.Framework;

namespace SharpAapt.Tests
{
    /// <summary>
    /// Integration tests skip when the Android SDK isn't installed, so they don't block local runs.
    /// CI sets SHARPAAPT_REQUIRE_SDK=true to fail instead, so a runner image change can't
    /// silently stop them from running.
    /// </summary>
    internal static class RequireSdk
    {
        public static void Missing(string message)
        {
            if (string.Equals(Environment.GetEnvironmentVariable("SHARPAAPT_REQUIRE_SDK"), "true", StringComparison.OrdinalIgnoreCase))
                Assert.Fail(message + " SHARPAAPT_REQUIRE_SDK is set, so this fails instead of skipping.");

            Assert.Ignore(message);
        }
    }
}
