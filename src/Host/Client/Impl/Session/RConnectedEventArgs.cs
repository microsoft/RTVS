// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Host.Client {
    public class RConnectedEventArgs : EventArgs {
        public string Name { get; }

        public RConnectedEventArgs(string name) {
            Name = name;
        }
    }
}