using System;

namespace SharpAapt
{
    /// <summary>
    /// Thrown when aapt exits with a non-zero exit code.
    /// </summary>
    public class AaptException : Exception
    {
        public AaptException(string message, int exitCode, string standardError)
            : base(message)
        {
            ExitCode = exitCode;
            StandardError = standardError;
        }

        /// <summary>
        /// The exit code returned by aapt.
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// Everything aapt wrote to standard error.
        /// </summary>
        public string StandardError { get; }
    }
}
