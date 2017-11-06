// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.R.Host.Protocol {
    public struct InterpreterInfo {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public string BinPath { get; set; }

        public string LibPath { get; set; }

        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }
    }
}
