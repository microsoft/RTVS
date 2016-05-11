// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.Plots;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Windows;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Plots {
    [Guid(WindowGuid)]
    internal class PlotWindowPane : RToolWindowPane, IVsWindowFrameNotify3, IPlotLocator {
        internal const string WindowGuid = "970AD71C-2B08-4093-8EA9-10840BC726A3";

        // Anything below 200 pixels at fixed 96dpi is impractical, and prone to rendering errors
        private const int MinPixelWidth = 200;
        private const int MinPixelHeight = 200;

        private IPlotHistory _plotHistory;
        private TaskCompletionSource<LocatorResult> _locatorTcs;

        public PlotWindowPane() {
            Caption = Resources.PlotWindowCaption;

            // this value matches with icmdShowPlotWindow's Icon in VSCT file
            BitmapImageMoniker = KnownMonikers.LineChart;

            var sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            var historyProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IPlotHistoryProvider>();

            _plotHistory = historyProvider.GetPlotHistory(sessionProvider.GetInteractiveWindowRSession());
            _plotHistory.PlotContentProvider.Locator = this;
            _plotHistory.HistoryChanged += OnPlotHistoryHistoryChanged;

            var presenter = new XamlPresenter(_plotHistory.PlotContentProvider);
            presenter.SizeChanged += PlotWindowPane_SizeChanged;

            Content = presenter;

            // initialize toolbar. Commands are added via package
            // so they appear correctly in the top level menu as well 
            // as on the plot window toolbar
            this.ToolBar = new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.plotWindowToolBarId);
        }

        private void PlotWindowPane_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e) {
            var unadjustedPixelSize = WpfUnitsConversion.ToPixels(Content as Visual, e.NewSize);

            // If the window gets below a certain minimum size, plot to the minimum size
            // and user will be able to use scrollbars to see the whole thing
            int pixelWidth = Math.Max((int)unadjustedPixelSize.Width, MinPixelWidth);
            int pixelHeight = Math.Max((int)unadjustedPixelSize.Height, MinPixelHeight);
            int resolution = WpfUnitsConversion.GetResolution(Content as Visual);

            // Throttle resize requests since we get a lot of size changed events when the tool window is undocked
            IdleTimeAction.Cancel(this);
            IdleTimeAction.Create(() => {
                PlotContentProvider.DoNotWait(_plotHistory.PlotContentProvider.ResizePlotAsync(pixelWidth, pixelHeight, resolution));
            }, 100, this);
        }

        private void RootContainer_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (_locatorTcs != null) {
                var rootContainer = (FrameworkElement)sender;
                var pos = e.GetPosition(rootContainer);
                var pixelSize = WpfUnitsConversion.ToPixels(rootContainer as Visual, pos);

                var result = LocatorResult.CreateClicked((int)pixelSize.X, (int)pixelSize.Y);
                EndLocatorMode(result);
            }
        }

        private void OnPlotHistoryHistoryChanged(object sender, EventArgs e) {
            ((IVsWindowFrame)Frame).ShowNoActivate();
            IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            shell.UpdateCommandUI(0);
        }

        protected override void Dispose(bool disposing) {
            _plotHistory?.Dispose();
            _plotHistory = null;
            base.Dispose(disposing);
        }

        #region IVsWindowFrameNotify3
        public int OnShow(int fShow) {
            return VSConstants.S_OK;
        }

        public int OnMove(int x, int y, int w, int h) {
            return VSConstants.S_OK;
        }

        public int OnSize(int x, int y, int w, int h) {
            return VSConstants.S_OK;
        }

        public int OnDockableChange(int fDockable, int x, int y, int w, int h) {
            return VSConstants.S_OK;
        }

        public int OnClose(ref uint pgrfSaveOptions) {
            return VSConstants.S_OK;
        }
        #endregion

        #region IPlotLocator
        public bool IsInLocatorMode {
            get { return _locatorTcs != null; }
        }

        public async Task<LocatorResult> StartLocatorModeAsync(CancellationToken ct) {
            _locatorTcs = new TaskCompletionSource<LocatorResult>();
            ct.Register(EndLocatorMode);

            await VsAppShell.Current.SwitchToMainThreadAsync();
            SetLocatorModeUI(true);

            var task = _locatorTcs.Task;
            return await task;
        }

        public void EndLocatorMode() {
            EndLocatorMode(LocatorResult.CreateNotClicked());
        }
        #endregion

        private void EndLocatorMode(LocatorResult result) {
            var tcs = _locatorTcs;
            _locatorTcs = null;
            tcs?.SetResult(result);
            VsAppShell.Current.DispatchOnUIThread(() => SetLocatorModeUI(false));
        }

        private void SetLocatorModeUI(bool locatorMode) {
            var rootContainer = ((XamlPresenter)Content).RootContainer;
            rootContainer.Cursor = locatorMode ? Cursors.Cross : Cursors.Arrow;
            if (locatorMode) {
                rootContainer.MouseLeftButtonUp += RootContainer_MouseLeftButtonUp;
            } else {
                rootContainer.MouseLeftButtonUp -= RootContainer_MouseLeftButtonUp;
            }

            var statusBar = VsAppShell.Current.GetGlobalService<IVsStatusbar>(typeof(SVsStatusbar));
            statusBar.SetText(locatorMode ? Resources.PlotWindowStatusLocatorActive : string.Empty);

            this.Caption = locatorMode ? Resources.PlotWindowCaptionLocatorActive : Resources.PlotWindowCaption;
        }
    }
}
