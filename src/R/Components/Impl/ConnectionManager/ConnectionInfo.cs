// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.ConnectionManager {
    public class ConnectionInfo : IConnectionInfo {
        public string Name { get; set; }
        public string Path { get; set; }
        public string RCommandLineArguments { get; set; }
        public bool IsUserCreated { get; set; }
        public DateTime LastUsed { get; set; }

        public ConnectionInfo() { }
        public ConnectionInfo(string name, string path, string rCommandLineArguments, DateTime lastUsed, bool isUserCreated) {
            Name = name;
            Path = path;
            RCommandLineArguments = rCommandLineArguments;
            IsUserCreated = isUserCreated;
            LastUsed = lastUsed;
        }
    }
}