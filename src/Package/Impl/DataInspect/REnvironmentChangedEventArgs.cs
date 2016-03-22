// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class REnvironmentChangedEventArgs : EventArgs {
        public REnvironmentChangedEventArgs(REnvironmentCollection environments) {
            Environments = environments;
        }

        public REnvironmentCollection Environments { get; }
    }
}
