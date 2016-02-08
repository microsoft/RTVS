using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Plots {
    [Guid(WindowGuid)]
    internal class PlotWindowPane : ToolWindowPane, IVsWindowFrameNotify3 {
        internal const string WindowGuid = "970AD71C-2B08-4093-8EA9-10840BC726A3";

        // Anything below 150 is impractical, and prone to rendering errors
        private const int MinWidth = 150;
        private const int MinHeight = 150;

        private IPlotHistory PlotHistory;

        public PlotWindowPane() {
            Caption = Resources.PlotWindowCaption;
            PlotHistory = VsAppShell.Current.ExportProvider.GetExportedValue<IPlotHistory>();
            PlotHistory.HistoryChanged += OnPlotHistoryHistoryChanged;

            var presenter = new XamlPresenter(PlotHistory.PlotContentProvider);
            presenter.SizeChanged += PlotWindowPane_SizeChanged;
            Content = presenter;

            // initialize toolbar. Commands are added via package
            // so they appear correctly in the top level menu as well 
            // as on the plot window toolbar
            this.ToolBar = new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.plotWindowToolBarId);
        }

        private void PlotWindowPane_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e) {
            // If the window gets below a certain minimum size, plot to the minimum size
            // and user will be able to use scrollbars to see the whole thing
            int width = Math.Max((int)e.NewSize.Width, MinWidth);
            int height = Math.Max((int)e.NewSize.Height, MinHeight);

            // Throttle resize requests since we get a lot of size changed events when the tool window is undocked
            IdleTimeAction.Cancel(this);
            IdleTimeAction.Create(() => {
                PlotContentProvider.DoNotWait(PlotHistory.PlotContentProvider.ResizePlotAsync(width, height));
            }, 100, this);
        }

        private void OnPlotHistoryHistoryChanged(object sender, EventArgs e) {
            ((IVsWindowFrame)Frame).ShowNoActivate();
            IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            shell.UpdateCommandUI(0);
        }

        protected override void Dispose(bool disposing) {
            PlotHistory?.Dispose();
            PlotHistory = null;
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
    }
}
