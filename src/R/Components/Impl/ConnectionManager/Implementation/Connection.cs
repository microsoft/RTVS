// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.ConnectionManager.Implementation {
    internal class Connection : ConnectionInfo, IConnection {
        public Connection(string name, string path, string rCommandLineArguments, DateTime lastUsed, bool isUserCreated): 
            base(name, path, rCommandLineArguments, lastUsed, isUserCreated) {
            Id = new Uri(path);
            IsRemote = !Id.IsFile;
        }

        public Uri Id { get; }

        /// <summary>
        /// If true, the connection is to a remote machine
        /// </summary>
        public bool IsRemote { get; }
    }
}