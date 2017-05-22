// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Common.Core.Extensions {
    public static class ReflectionExtensions {
        public static IDictionary<string, object> GetPropertyValueDictionary(this object o) {
            var dict = new Dictionary<string, object>();
            var properties = o.GetType().GetTypeInfo().GetProperties();
            foreach (var p in properties) {
                var value = p.GetValue(o);
                dict[p.Name] = value;
            }
            return dict;
        }

        public static void SetProperties(this object o, IDictionary<string, object> dict) {
            var properties = o.GetType().GetTypeInfo().GetProperties();
            foreach (var p in properties) {
                if (dict.TryGetValue(p.Name, out var value)) {
                    p.SetValue(o, value);
                }
            }
        }
    }
}
