// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal sealed class VsIdleTimeService : IIdleTimeService {
        #region IIdleTimeService
        /// <summary>
        /// Fires when host application enters idle state.
        /// </summary>
        public event EventHandler<EventArgs> Idle;
        #endregion

        public void FireIdle() => Idle?.Invoke(this, EventArgs.Empty);
    }
}
