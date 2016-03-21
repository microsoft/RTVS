// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.DataInspect.Definitions;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class REnvironmentProvider : IREnvironmentProvider {
        private IRSession _rSession;

        public REnvironmentProvider(IRSession session) {
            _rSession = session;
        }

        #region IREnvironmentProvider

        public event EventHandler<REnvironmentChangedEventArgs> EnvironmentChanged;

        #endregion
    }
}
