// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core.Shell {
    public interface IIdleTimeService {
        /// <summary>
        /// Fires when host application enters idle state.
        /// </summary>
        event EventHandler<EventArgs> Idle;

        /// <summary>
        /// Fires when application is closing
        /// </summary>
        event EventHandler<EventArgs> Closing;
    }
}

