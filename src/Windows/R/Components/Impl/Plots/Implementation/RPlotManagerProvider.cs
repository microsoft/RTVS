// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation {
    [Export(typeof(IRPlotManagerProvider))]
    internal class RPlotManagerProvider : IRPlotManagerProvider {
        public IRPlotManager CreatePlotManager(IRInteractiveWorkflow interactiveWorkflow) {
            Check.InvalidOperation(() => interactiveWorkflow is IRInteractiveWorkflowVisual);
            return new RPlotManager(interactiveWorkflow as IRInteractiveWorkflowVisual);
        }
    }
}
