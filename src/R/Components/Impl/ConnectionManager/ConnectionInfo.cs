// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.R.Components.ConnectionManager {
    public class ConnectionInfo : IConnectionInfo {
        public string Name { get; }
        public string Path { get; }
        public string RCommandLineArguments { get; }
        public bool IsUserCreated { get; }
        public DateTime LastUsed { get; set; }

        public ConnectionInfo(IConnectionInfo connectionInfo) :
            this(connectionInfo.Name, connectionInfo.Path, connectionInfo.RCommandLineArguments, connectionInfo.LastUsed, connectionInfo.IsUserCreated) { }

        public ConnectionInfo(string name, string path, string rCommandLineArguments, bool isUserCreated) :
            this(name, path, rCommandLineArguments, DateTime.MinValue, isUserCreated) { }

        [JsonConstructor]
        private ConnectionInfo(string name, string path, string rCommandLineArguments, DateTime lastUsed, bool isUserCreated) {
            Name = name;
            Path = path;
            RCommandLineArguments = rCommandLineArguments;
            IsUserCreated = isUserCreated;
            LastUsed = lastUsed;
        }
    }
}