using System;

namespace Microsoft.R.Core.Tokens
{
    internal static class Logicals
    {
        public static bool IsLogical(string candidate)
        {
            // R is case sensitive language
             return Array.BinarySearch<string>(_logicals, candidate) >= 0;
        }

        // must be sorted
        internal static string[] _logicals = new string[]
        {
            "F",
            "FALSE",
            "T",
            "TRUE",
        };
    }
}
