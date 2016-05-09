// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Common.Core {
    public static class CultureExtensions {
        private static readonly char[] _separators = new char[] { ',', ' ' };

        public static string LanguageNameFromLCID(int lcid) {
            var culture = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures).FirstOrDefault(x => x.LCID == lcid);
            return culture.GetLanguageName();
        }

        public static IEnumerable<string> GetLanguageNames() {
            IEnumerable<CultureInfo> cultures = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures);
            return cultures.Select(c => c.GetLanguageName()).Distinct().OrderBy(x => x);
        }

        private static string GetLanguageName(this CultureInfo culture) {
            if (culture != null) {
                var index = culture.DisplayName.IndexOfAny(_separators);
                return index >= 0 ? culture.DisplayName.Substring(0, index).TrimEnd() : culture.DisplayName;
            }
            return null;
        }
    }
}
