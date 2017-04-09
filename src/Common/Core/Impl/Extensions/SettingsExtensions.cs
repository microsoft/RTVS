// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Common.Core.Shell;

namespace Microsoft.Common.Core.Extensions {
    public static class SettingsExtensions {
        public static void LoadPropertyValues(this ISettingsStorage settings, object o) {
            var properties = o.GetType().GetTypeInfo().GetProperties();
            foreach (var p in properties) {
                if (settings.SettingExists(p.Name)) {
                    try {
                        var value = settings.GetSetting(p.Name, p.PropertyType);
                        p.SetValue(o, value);
                    } catch(ArgumentException) { } // protect from settings store damage
                }
            }
        }

        public static void SavePropertyValues(this ISettingsStorage settings, object o) {
            var dict = o.GetPropertyValueDictionary();
            foreach (var kvp in dict) {
                settings.SetSetting(kvp.Key, kvp.Value);
            }
        }
    }
}
