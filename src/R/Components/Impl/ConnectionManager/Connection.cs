// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.ConnectionManager {
    internal class Connection : ConnectionInfo, IConnection {
        public Connection(IConnectionInfo ci) :
            this(ci.Name, ci.Path, ci.RCommandLineArguments, ci.LastUsed, ci.IsUserCreated) { }

        public Connection(string name, string path, string rCommandLineArguments, DateTime lastUsed, bool isUserCreated) :
            base(name, path, rCommandLineArguments, lastUsed, isUserCreated) {
            Uri = new Uri(path);
            IsRemote = !Uri.IsFile;
        }

        public Uri Uri { get; }

        /// <summary>
        /// If true, the connection is to a remote machine
        /// </summary>
        public bool IsRemote { get; }
    }
}