// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.R.Components.Plots;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;

namespace Microsoft.VisualStudio.R.Package.Plots {
    internal sealed class PlotHistory : IPlotHistory {

        public PlotHistory(IRSession session) {
            ActivePlotIndex = -1;
            PlotContentProvider = new PlotContentProvider(session);
            PlotContentProvider.PlotChanged += OnPlotChanged;
        }

        #region IPlotHistory
        public int ActivePlotIndex { get; private set; }
        public int PlotCount { get; private set; }
        public IPlotContentProvider PlotContentProvider { get; private set; }

        public async Task RefreshHistoryInfo() {
            var info = await PlotContentProvider.GetHistoryInfoAsync();
            ActivePlotIndex = info.ActivePlotIndex;
            PlotCount = info.PlotCount;

            VsAppShell.Current.DispatchOnUIThread(() => {
                // We need to push creation of the tool window
                // so it appears on the first plot
                if (!VsAppShell.Current.IsUnitTestEnvironment) {
                    ToolWindowUtilities.ShowWindowPane<PlotWindowPane>(0, false);
                }
                HistoryChanged?.Invoke(this, EventArgs.Empty);
            });
        }

        public event EventHandler HistoryChanged;
        #endregion

        private void OnPlotChanged(object sender, PlotChangedEventArgs e) {
            if (e.NewPlotElement == null) {
                ClearHistoryInfo();
            } else {
                Plots.PlotContentProvider.DoNotWait(RefreshHistoryInfo());
            }
        }

        private void ClearHistoryInfo() {
            ActivePlotIndex = -1;
            PlotCount = 0;
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose() {
            if (PlotContentProvider != null) {
                PlotContentProvider.PlotChanged -= OnPlotChanged;
                PlotContentProvider.Dispose();
                PlotContentProvider = null;
            }
        }
    }
}
