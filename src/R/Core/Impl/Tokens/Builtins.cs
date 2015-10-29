using System;

namespace Microsoft.R.Core.Tokens {
    internal static class Builtins {
        public static bool IsBuiltin(string candidate) {
            // R is case sensitive language
            return Array.BinarySearch<string>(_builtins, candidate) >= 0;
        }

        public static string[] BuiltinList {
            get { return _builtins; }
        }

        internal static string[] _builtins = {
            "library",
            "require",
            "return",
            "switch",
            "typeof",
        };
    }
}
