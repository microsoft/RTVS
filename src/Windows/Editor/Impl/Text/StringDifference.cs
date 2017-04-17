// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if NOT_USED
using System;

namespace Microsoft.Languages.Editor.Text
{
    public class StringDifference
    {
        public int Start { get; set; }
        public int OldLength { get; set; }
        public int NewLength { get; set; }

        public StringDifference(int start, int oldLength, int newLength)
        {
            Start = start;
            OldLength = oldLength;
            NewLength = newLength;
        }

        public static StringDifference Difference(string value1, string value2)
        {
            int minLen = Math.Min(value1.Length, value2.Length);
            int i, j;

            int start = 0;

            for (i = 0; i < minLen; i++)
            {
                if (value1[i] != value2[i])
                {
                    start = i;
                    break;
                }
            }

            if (i == minLen)
            {
                if (value1.Length == value2.Length)
                    return new StringDifference(0, 0, 0);

                if (value1.Length > value2.Length)
                    return new StringDifference(value2.Length, value1.Length - value2.Length, 0);

                return new StringDifference(value1.Length, 0, value2.Length - value1.Length);
            }

            for (i = value1.Length - 1, j = value2.Length - 1; i > start && j > start; i--, j--)
            {
                if (value1[i] != value2[j])
                    break;
            }

            return new StringDifference(start, i - start + 1, j - start + 1);
        }
    }
}
#endif
