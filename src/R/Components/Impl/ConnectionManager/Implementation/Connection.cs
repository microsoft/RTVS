// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.ConnectionManager.Implementation {
    internal class Connection : IConnection {
        public Connection(string name, string path, string rCommandLineArguments, DateTime timeStamp) {
            Id = new Uri(path);
            Name = name;
            Path = path;
            IsRemote = !Id.IsFile;
            TimeStamp = timeStamp;
            RCommandLineArguments = rCommandLineArguments;
        }

        public Uri Id { get; }
        public string Name { get; }
        public string Path { get; }
        public string RCommandLineArguments { get; }
        public bool IsRemote { get; }
        public DateTime TimeStamp { get; }
    }
}