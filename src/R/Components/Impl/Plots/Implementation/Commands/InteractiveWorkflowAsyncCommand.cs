// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal abstract class InteractiveWorkflowAsyncCommand {
        protected IRInteractiveWorkflow InteractiveWorkflow { get; }

        public InteractiveWorkflowAsyncCommand(IRInteractiveWorkflow interactiveWorkflow) {
            if (interactiveWorkflow == null) {
                throw new ArgumentNullException(nameof(interactiveWorkflow));
            }

            InteractiveWorkflow = interactiveWorkflow;
        }
    }
}
