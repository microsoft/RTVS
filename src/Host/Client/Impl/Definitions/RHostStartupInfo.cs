// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    public class RHostStartupInfo {
        public string Name { get; set; }
        public string RBasePath { get; set; }
        public string RHostDirectory { get; set; }
        public string RHostCommandLineArguments { get; set; }
        public string CranMirrorName { get; set; }
        public string WorkingDirectory { get; set; }
        public int CodePage { get; set; }
        public int TerminalWidth { get; set; } = 80;
    }
}
