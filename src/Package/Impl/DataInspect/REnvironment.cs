// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class REnvironment {
        public REnvironment(string name) {
            Name = name;
        }

        public REnvironment(JToken token) {
            var name = token.Value<string>("name");
            if (name != null) {
                Name = name;
            } else {
                throw new ArgumentException("JSon doesn't have value with key 'name'");
            }

            var frameIndex = token.Value<int?>("frameindex");
            if (frameIndex != null) {
                FrameIndex = frameIndex;
            }
        }

        public string Name { get; }

        public int? FrameIndex { get; }
    }
}
