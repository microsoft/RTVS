// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;

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

        /// <summary>
        /// Checks if two connection infos have equal path and command line arguments,
        /// hence will create identical connection to the broker
        /// </summary>
        /// <returns>True if connections are identical</returns>
        public static bool AreIdentical(IConnectionInfo connectionInfo1, IConnectionInfo connectionInfo2) {
            if (connectionInfo1 == null && connectionInfo2 == null) {
                return true;
            }

            if (connectionInfo1 != null && connectionInfo2 != null) {
                if (!connectionInfo1.Path.PathEquals(connectionInfo2.Path)) {
                    return false;
                }

                if (string.IsNullOrEmpty(connectionInfo1.RCommandLineArguments) && string.IsNullOrEmpty(connectionInfo2.RCommandLineArguments)) {
                    return true;
                }

                return connectionInfo1.RCommandLineArguments.EqualsIgnoreCase(connectionInfo2.RCommandLineArguments);
            }

            return false;
        }
    }
}