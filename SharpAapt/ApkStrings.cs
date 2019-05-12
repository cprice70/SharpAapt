using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace SharpAapt
{
    public class ApkStrings
    {
        public ApkStrings(string input)
        {
            try
            {
                var results = input.Split('\n');

                foreach (var result in results)
                {
                    var values = result.Split(':');
                    if (values.Length < 2) continue;
                    if (string.IsNullOrEmpty(values[0]) || string.IsNullOrEmpty(values[1])) continue;
                    Strings.Add(values[0].Trim(), values[1].Trim());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        
        public Dictionary<string, string> Strings { get; } = new Dictionary<string, string>();
    }
}