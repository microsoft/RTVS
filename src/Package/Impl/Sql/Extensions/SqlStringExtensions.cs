// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Sql {
    internal static class SqlStringExtensions {
        // https://technet.microsoft.com/en-us/library/ms176027%28v=sql.105%29.aspx
        public static string ToSqlName(this string name, SqlQuoteType quoteType) {
            if(name.HasSpaces() && quoteType == SqlQuoteType.None) {
                quoteType = SqlQuoteType.Bracket;
            }
            switch (quoteType) {
                case SqlQuoteType.Quote:
                    return Invariant($"\"{name}\"");
                case SqlQuoteType.Bracket:
                    return Invariant($"[{name}]");
                default:
                    return name;
            }
        }

        private static bool HasSpaces(this string s) {
            return s.Any(c => char.IsWhiteSpace(c));
        }
    }
}
