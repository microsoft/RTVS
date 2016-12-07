// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.ConnectionManager {
    public interface IConnection: IConnectionInfo {
        Uri Uri { get; }

        /// <summary>
        /// If true, the connection is to a remote machine
        /// </summary>
        bool IsRemote { get; }
    }
}