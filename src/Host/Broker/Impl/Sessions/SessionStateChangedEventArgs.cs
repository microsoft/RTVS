// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.Sessions {
    public class SessionStateChangedEventArgs : EventArgs {
        public SessionState OldState { get; }
        public SessionState NewState { get; }

        public SessionStateChangedEventArgs(SessionState oldState, SessionState newState) {
            OldState = oldState;
            NewState = newState;
        }
    }
}
