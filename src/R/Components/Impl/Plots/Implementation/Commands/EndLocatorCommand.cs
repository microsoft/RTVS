// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class EndLocatorCommand : PlotCommand, IAsyncCommand {
        public EndLocatorCommand(IRInteractiveWorkflow interactiveWorkflow) : base(interactiveWorkflow) {
        }

        public CommandStatus Status {
            get {
                if (IsInLocatorMode) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported | CommandStatus.Invisible;
            }
        }

        public Task<CommandResult> InvokeAsync() {
            InteractiveWorkflow.Plots.EndLocatorMode();
            return Task.FromResult(CommandResult.Executed);
        }
    }
}
