using System;

namespace Microsoft.Languages.Core.Utility
{
    public static class StringUtility
    {
        public static int GetMatchingPrefixLen(this string s1, string s2)
        {
            int matchingLen = 0;
            int maxMatchingLen = Math.Min(s1.Length, s2.Length);

            while (matchingLen < maxMatchingLen)
            {
                if (s1[matchingLen] != s2[matchingLen])
                {
                    break;
                }

                matchingLen++;
            }

            return matchingLen;
        }
    }
}
