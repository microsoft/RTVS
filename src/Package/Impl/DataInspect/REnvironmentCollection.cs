// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class REnvironmentCollection : List<REnvironment>{
        public REnvironmentCollection() { }

        public REnvironmentCollection(JToken token) {
            var jarray = token as JArray;
            if (jarray != null) {
                foreach (var jsonItem in jarray) {
                    Add(new REnvironment(jsonItem));
                }
            }
        }
    }
}
