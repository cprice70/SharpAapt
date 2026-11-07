using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SharpAapt
{
    public static class RegexHelpers
    {
        /// <summary>
        /// Matches the contents of each single-quoted value on a line. Values may contain spaces
        /// but must not start with whitespace.
        /// </summary>
        public const string ValuePattern = @"(?<=')[^'\s][^']*(?=')";
        public static Regex ValueRegex { get; } = new Regex(ValuePattern, RegexOptions.Compiled);
        public const string NamePattern = @"(?<=name\=\')(.*?)(?=\')";
        public static readonly Regex NameRegex = new Regex(NamePattern, RegexOptions.Compiled);
        public const string ReasonPattern = @"(?<=reason\=\')(.*?)(?=\')";
        public static readonly Regex ReasonRegex = new Regex(ReasonPattern, RegexOptions.Compiled);

        private static readonly Regex QuotedRegex = new Regex(@"'([^']*)'", RegexOptions.Compiled);

        /// <summary>
        /// Returns every single-quoted value on a line, in order, without the quotes.
        /// </summary>
        internal static List<string> QuotedValues(string line) =>
            QuotedRegex.Matches(line).Cast<Match>().Select(m => m.Groups[1].Value).ToList();

        /// <summary>
        /// Returns the first single-quoted value on a line, or an empty string.
        /// </summary>
        internal static string FirstQuotedValue(string line)
        {
            var match = QuotedRegex.Match(line);
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        /// <summary>
        /// Returns the value of <c>key='value'</c> on a line, or null if the key is absent.
        /// </summary>
        internal static string Attribute(string line, string key)
        {
            var match = Regex.Match(line, @"(?:^|[\s:])" + Regex.Escape(key) + @"='([^']*)'");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
