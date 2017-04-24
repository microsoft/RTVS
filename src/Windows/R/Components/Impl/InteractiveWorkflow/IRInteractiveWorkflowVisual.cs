// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.R.Components.History;

namespace Microsoft.R.Components.InteractiveWorkflow {
    public interface IRInteractiveWorkflowVisual : IRInteractiveWorkflow {
        IRHistory History { get; }
        IInteractiveWindowVisualComponent ActiveWindow { get; }

        event EventHandler<ActiveWindowChangedEventArgs> ActiveWindowChanged;

        Task<IInteractiveWindowVisualComponent> GetOrCreateVisualComponentAsync(int instanceId = 0);
    }
}