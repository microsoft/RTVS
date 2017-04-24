// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Common.Core;

namespace Microsoft.R.Components.Application.Configuration {
    public static class ConfigurationSettingCollectionExtensions {
        public static string FindNextAvailableSettingName(this IConfigurationSettingCollection settings, string nameTemplate) {
            var names = new HashSet<string>();
            var existingNames = settings.Where(s => s.Name.StartsWithOrdinal(nameTemplate)).Select(s => s.Name);
            foreach (var n in existingNames) {
                names.Add(n);
            }
            if (names.Count > 0) {
                for (int i = 1; i < 1000; i++) {
                    var candidate = nameTemplate + i.ToString(CultureInfo.InvariantCulture);
                    if (!names.Contains(candidate)) {
                        return candidate;
                    }
                }
            }
            return nameTemplate;
        }
    }
}
