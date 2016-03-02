// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Test.Utility
{
    [ExcludeFromCodeCoverage]
    public static class TextRangeCollectionWriter
    {
        public static string WriteCollection<T>(ITextRangeCollection<T> ranges) where T : ITextRange
        {
            var sb = new StringBuilder();
            int i = 0;

            foreach (ITextRange r in ranges)
            {
                sb.AppendFormat("[{0}][{1}...{2}), Length = {3}\r\n", i, r.Start, r.End, r.Length);
                i++;
            }

            return sb.ToString();
        }
    }
}
