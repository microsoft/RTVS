using System;
using System.Text;

namespace Microsoft.Languages.Core.Text
{
    public static class StringExtensions
    {
        public static string TrimQuotes(this string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                char firstChar = s[0];
                bool startsWithQuote = firstChar == '\"' || firstChar == '\'';
                int trimmedLength = s.Length - (startsWithQuote ? 1 : 0);

                if (s.Length > 1)
                {
                    bool endsWithQuote = false;
                    char lastChar = s[s.Length - 1];
                    if (startsWithQuote)
                    {
                        endsWithQuote = (lastChar == firstChar);
                    }
                    else
                    {
                        endsWithQuote = (lastChar == '\"' || lastChar == '\'');
                    }

                    trimmedLength -= (endsWithQuote ? 1 : 0);
                }

                return s.Substring(startsWithQuote ? 1 : 0, trimmedLength);
            }

            return s;
        }

        public static string AddQuotes(this string s)
        {
            StringBuilder sb = new StringBuilder(s.Length + 2);

            sb.Append('"');
            sb.Append(s);
            sb.Append('"');

            return sb.ToString();
        }

        public static bool IsQuoted(this string s)
        {
            return s.Length > 1 && s[0] == '\"' && s[s.Length-1] == '\"';
        }
    }
}
