// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.Settings {
    public class ConnectionInfo {
        public string Name { get; set; }
        public string Path { get; set; }
        public string RCommandLineArguments { get; set; }
        public bool IsUserCreated { get; set; }
    }
}