// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Common.Core;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class CopyPlotAsBitmapCommand : PlotCommand, IAsyncCommand {
        public CopyPlotAsBitmapCommand(IRInteractiveWorkflow interactiveWorkflow) : base(interactiveWorkflow) {
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
                await InteractiveWorkflow.Plots.ExportToBitmapAsync("bmp", filePath);

                InteractiveWorkflow.Shell.DispatchOnUIThread(() => {
                    try {
                        // Use Begin/EndInit to avoid locking the file on disk
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.UriSource = new Uri(filePath);
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.EndInit();
                        Clipboard.SetImage(image);
                    } catch (Exception e) when (!e.IsCriticalException()) {
                        MessageBox.Show(string.Format(Resources.Plots_CopyToClipboardError, e.Message));
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
