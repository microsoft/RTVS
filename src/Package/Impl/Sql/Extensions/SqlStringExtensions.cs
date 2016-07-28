// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Sql {
    internal static class SqlStringExtensions {
        public static string ToSqlName(this string name, SqlQuoteType quoteType) {
            if (!HasSpaces(name)) {
                return name;
            }
            if (quoteType == SqlQuoteType.Quote) {
                return Invariant($"\"{name}\"");
            }
            return Invariant($"[{name}]");
        }

        private static bool HasSpaces(this string s) {
            return s.FirstOrDefault(c => char.IsWhiteSpace(c)) != default(char);
        }
    }
}
