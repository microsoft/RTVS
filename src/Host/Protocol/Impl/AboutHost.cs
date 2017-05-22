// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.R.Host.Protocol {
    public class AboutHost {
        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }
        public string OSDescription { get; set; }
        public bool Is64BitOperatingSystem { get; set; }
        public bool Is64BitProcess { get; set; }
        public int ProcessorCount { get; set; }
        public long WorkingSet { get; set; }


        public long TotalVirtualMemory { get; set; }
        public long FreeVirtualMemory { get; set; }
        public long TotalPhysicalMemory { get; set; }
        public long FreePhysicalMemory { get; set; }

        public IEnumerable<VideoCardInfo> VideoCards { get; set; }

        public int ConnectedUserCount { get; set; }

        public string[] Interpreters { get; set; }

        public static AboutHost Empty => new AboutHost {
            Interpreters = new string[0],
            Version = new Version(0, 0),
            OSDescription = RuntimeInformation.OSDescription,
            Is64BitOperatingSystem = true,
            Is64BitProcess = true,
        };
    }
}
