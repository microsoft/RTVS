// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Commands {
    public sealed class ShowMainPlotWindowCommand : IAsyncCommand {
        private readonly IRInteractiveWorkflow _workflow;

        public ShowMainPlotWindowCommand(IRInteractiveWorkflow workflow) {
            Check.ArgumentNull(nameof(workflow), workflow);
            _workflow = workflow;
        }

        public CommandStatus Status => CommandStatus.SupportedAndEnabled;

        public Task InvokeAsync() {
            try {
                var component = ((IRPlotManagerVisual)_workflow.Plots).GetOrCreateMainPlotVisualComponent();
                component.Container.Show(focus: true, immediate: false);
            } catch (RPlotManagerException ex) {
                _workflow.Shell.ShowErrorMessage(ex.Message);
            } catch (OperationCanceledException) {
            }

            return Task.CompletedTask;
        }
    }
}
