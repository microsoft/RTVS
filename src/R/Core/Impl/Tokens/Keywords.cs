using System;

namespace Microsoft.R.Core.Tokens
{
    public static class Keywords
    {
        public static string[] KeywordList
        {
            get { return _keywords; }
        }

        public static bool IsKeyword(string candidate)
        {
            // R is case sensitive language
            return Array.BinarySearch<string>(_keywords, candidate) >= 0;
        }

        internal static string[] _keywords = new string[]
        {
            "break",
            "else",
            "for",
            "function",
            "if",
            "in",
            "library",
            "next",
            "repeat",
            "return",
            "while",
        };
    }
}
