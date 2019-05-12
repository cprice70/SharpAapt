using System.Text.RegularExpressions;

namespace SharpAapt
{
    public static class RegexHelpers
    {
        public const string ValuePattern = @"(?<=\')(\S+)(?=\')";
        public static Regex ValueRegex => new Regex(ValuePattern);
        public const string NamePattern = @"(?<=name\=\')(.*?)(?=\')";
        public static Regex NameRegex = new Regex(NamePattern);
        public const string ReasonPattern = @"(?<=reason\=\')(.*?)(?=\')";
        public static Regex ReasonRegex = new Regex(ReasonPattern);
    }
}