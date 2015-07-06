using System;

namespace Microsoft.R.Core.Tokens
{
    internal static class Constants
    {
        public static bool IsConstant(string candidate)
        {
            // R is case sensitive language
             return Array.BinarySearch<string>(_constants, candidate) >= 0;
        }

        // must be sorted
        internal static string[] _constants = new string[]
        {
            "Inf",
            "NA",
            "NaN",
            "NULL",
        };
    }
}
