// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.ConnectionManager.Implementation {
    internal class Connection : IConnection {
        public Connection(string name, string path, string rCommandLineArguments, DateTime timeStamp, bool isUserCreated) {
            Id = new Uri(path);
            Name = name;
            Path = path;
            RCommandLineArguments = rCommandLineArguments;
            IsUserCreated = isUserCreated;
            TimeStamp = timeStamp;

            IsRemote = !Id.IsFile;
        }

        public Uri Id { get; }

        /// <summary>
        /// User-friendly name of the connection.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Path to local interpreter installation or URL to remote machine.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Optional command line arguments to R interpreter.
        /// </summary>
        public string RCommandLineArguments { get; }

        /// <summary>
        /// If true, the connection is to a remote machine
        /// </summary>
        public bool IsRemote { get; }
        
        /// <summary>
        /// Indicates that this is user-created connection rather than
        /// automatically created one when interpreter has been detected 
        /// in registry or via other means automatically.
        /// </summary>
        public bool IsUserCreated { get; set; }

        /// <summary>
        /// When was the connection last used.
        /// </summary>
        public DateTime TimeStamp { get; }
    }
}