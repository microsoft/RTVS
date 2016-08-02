// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.ConnectionManager {
    public class ConnectionEventArgs : EventArgs {
        public IConnection Connection { get; }
        public bool State { get; private set; }

        public ConnectionEventArgs(bool state, IConnection connection) {
            State = state;
            Connection = connection;
        }
    }
}
