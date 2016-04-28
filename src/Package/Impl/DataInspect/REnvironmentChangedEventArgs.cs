// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class REnvironmentChangedEventArgs : EventArgs {
        public REnvironmentChangedEventArgs(IReadOnlyList<REnvironment> environments) {
            Environments = environments;
        }

        public IReadOnlyList<REnvironment> Environments { get; }
    }
}
