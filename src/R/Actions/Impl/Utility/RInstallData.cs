// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Actions.Utility {
    public class RInstallData {
        public RInstallStatus Status { get; set; }
        public string Path { get; set; }
        public Version Version { get; set; }

        public Exception Exception { get; set; }
    }
}
