// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.ConnectionManager {
    public interface IConnectionInfo {
        /// <summary>
        /// User-friendly name of the connection.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Path to local interpreter installation or URL to remote machine.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Optional command line arguments to R interpreter.
        /// </summary>
        string RCommandLineArguments { get; }

        /// <summary>
        /// Indicates that this is user-created connection rather than
        /// automatically created one when interpreter has been detected 
        /// in registry or via other means automatically.
        /// </summary>
        bool IsUserCreated { get; }

        /// <summary>
        /// When was the connection last used.
        /// </summary>
        DateTime LastUsed { get; set; }
    }
}