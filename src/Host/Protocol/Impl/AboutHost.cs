// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.R.Host.Protocol {
    public class AboutHost {
        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }
        public OperatingSystem OS { get; set; }
        public bool Is64BitOperatingSystem { get; set; }
        public bool Is64BitProcess { get; set; }
        public int ProcessorCount { get; set; }
        public long WorkingSet { get; set; }
        public long TotalVirtualMemory { get; set; }
        public long FreeVirtualMemory { get; set; }
        public long TotalPhysicalMemory { get; set; }
        public long FreePhysicalMemory { get; set; }
        public int ConnectedUserCount { get; set; }

        public string[] Interpreters { get; set; }

        public static AboutHost Empty {
            get {
                var a = new AboutHost() {
                    Interpreters = new string[0],
                    Version = new Version(),
                    OS = Environment.OSVersion,
                    Is64BitOperatingSystem = true,
                    Is64BitProcess = true,
                };
                return a;
            }
        }
    }
}
