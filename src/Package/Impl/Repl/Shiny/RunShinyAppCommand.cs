// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Shiny {
    internal sealed class RunShinyAppCommand : PackageCommand {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;

        public static Task RunningTask { get; private set; }

        public RunShinyAppCommand(IRInteractiveWorkflow interactiveWorkflow)
            : base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRunShinyApp) {
            _interactiveWorkflow = interactiveWorkflow;
        }

        protected override void SetStatus() {
            Visible = true;
            Enabled = _interactiveWorkflow.RSession.IsHostRunning && RunningTask == null;
        }

        protected override void Handle() {
            RunningTask = Task.Run(async () => {
                try {
                    using (var e = await _interactiveWorkflow.RSession.BeginInteractionAsync()) {
                        await e.RespondAsync("library(shiny)" + Environment.NewLine + "runApp()" + Environment.NewLine);
                    }
                } catch (TaskCanceledException) { } catch (MessageTransportException) { }
            }).ContinueWith((t) => RunningTask = null);
        }
    }
}
