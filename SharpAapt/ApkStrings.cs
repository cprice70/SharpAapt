using System;
using System.Collections.Generic;

namespace SharpAapt
{
    public class ApkStrings
    {
        public ApkStrings(string input)
        {
            var results = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var result in results)
            {
                // Lines look like "String #12 : value". Split on the first colon only so values keep theirs.
                var values = result.Split(new[] { ':' }, 2);
                if (values.Length < 2) continue;

                var key = values[0].Trim();
                var value = values[1].Trim();
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) continue;

                Strings[key] = value;
            }
        }

        public Dictionary<string, string> Strings { get; } = new Dictionary<string, string>();
    }
}
