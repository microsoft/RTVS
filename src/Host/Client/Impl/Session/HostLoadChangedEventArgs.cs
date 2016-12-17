// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Client {
    public class HostLoadChangedEventArgs : EventArgs {
        public HostLoad HostLoad { get; }

        public HostLoadChangedEventArgs(HostLoad hostLoad) {
            HostLoad = hostLoad;
        }
    }
}