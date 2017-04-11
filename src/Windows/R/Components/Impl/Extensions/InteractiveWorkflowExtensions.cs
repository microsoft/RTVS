// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.R.Components.InteractiveWorkflow {
    public static class InteractiveWorkflowExtensions {
        public static Task<IInteractiveWindowVisualComponent> GetOrCreateVisualComponentAsync(this IRInteractiveWorkflow workflow, int instanceId = 0)
            => ((IRInteractiveWorkflowVisual)workflow).GetOrCreateVisualComponentAsync(instanceId);
    }
}
