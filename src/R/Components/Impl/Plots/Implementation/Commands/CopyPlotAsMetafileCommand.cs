// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Common.Core;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class CopyPlotAsMetafileCommand : PlotCommand, IAsyncCommand {
        public CopyPlotAsMetafileCommand(IRInteractiveWorkflow interactiveWorkflow) : base(interactiveWorkflow) {
        }

        public CommandStatus Status {
            get {
                if (HasCurrentPlot && !IsInLocatorMode) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported;
            }
        }

        public async Task<CommandResult> InvokeAsync() {
            string filePath = Path.GetTempFileName();
            try {
                await InteractiveWorkflow.Plots.ExportToMetafileAsync(filePath);

                InteractiveWorkflow.Shell.DispatchOnUIThread(() => {
                    try {
                        var mf = new System.Drawing.Imaging.Metafile(filePath);
                        Clipboard.SetData(DataFormats.EnhancedMetafile, mf);
                    } catch (Exception e) when (!e.IsCriticalException()) {
                        InteractiveWorkflow.Shell.ShowErrorMessage(string.Format(Resources.Plots_CopyToClipboardError, e.Message));
                    } finally {
                        try {
                            File.Delete(filePath);
                        } catch (IOException) {
                        }
                    }
                });
            } catch (RPlotManagerException ex) {
                InteractiveWorkflow.Shell.ShowErrorMessage(ex.Message);
            } catch (OperationCanceledException) {
            }

            return CommandResult.Executed;
        }
    }
}
