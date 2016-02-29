// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    public class RHostStartupInfo {
        public string Name { get; set; }
        public string RBasePath { get; set; }
        public string RCommandLineArguments { get; set; }
        public string CranMirrorName { get; set; }
        public string WorkingDirectory { get; set; }
    }
}
