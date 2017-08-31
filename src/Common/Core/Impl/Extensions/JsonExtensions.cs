// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Common.Core {
    public static class JsonExtensions {
        public static IEnumerable<T> GetEnumerable<T>(this JToken token, string key) {
            var array = token.Value<JArray>(key);
            return array.HasValues ? array.Children<JValue>().Select(v => (T)v.Value) : Enumerable.Empty<T>();
        }
    }
}
