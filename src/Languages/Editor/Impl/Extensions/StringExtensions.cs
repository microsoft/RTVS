// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;

namespace Microsoft.Languages.Editor.Utility {
    public static class StringExtensions {
        /// <summary>
        /// Wraps long string so each line is no longer
        /// that the specified number of characters 
        /// </summary>
        /// <returns></returns>
        public static string Wrap(this string s, int limit) {
            limit = Math.Max(80, limit);

            StringBuilder sb = new StringBuilder();
            int count = 0;

            for (int i = 0; i < s.Length; i++) {
                char ch = s[i];

                if (char.IsWhiteSpace(ch) && count >= limit) {
                    sb.Append("\r\n");
                    count = 0;
                } else {
                    sb.Append(ch);
                    count++;
                }
            }

            return sb.ToString();
        }
    }
}
