using System;

namespace Microsoft.R.Core.Tokens
{
    internal static class Builtins
    {
        public static bool IsBuiltin(string candidate)
        {
            // R is case sensitive language
            return Array.BinarySearch<string>(_builtins, candidate) >= 0;
        }

        internal static string[] _builtins = new string[]
        {
            "library",
            "switch",
            "typeof",
        };
    }
}
