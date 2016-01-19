using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Controller;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Plots.Commands;
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

        private PlotWindowCommand[] _copyAndExportCommands;
        private HistoryNextPlotCommand _historyNextPlotCommand;
        private HistoryPreviousPlotCommand _historyPreviousPlotCommand;

        public PlotWindowPane() {
            Caption = Resources.PlotWindowCaption;

            // set content with presenter
            PlotContentProvider = new PlotContentProvider();
            PlotContentProvider.PlotChanged += ContentProvider_PlotChanged;


            var presenter = new XamlPresenter(PlotContentProvider);
            presenter.SizeChanged += PlotWindowPane_SizeChanged;
            Content = presenter;

            // initialize toolbar
            this.ToolBar = new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.plotWindowToolBarId);

            Controller c = new Controller();
            c.AddCommandSet(GetCommands());
            this.ToolBarCommandTarget = new CommandTargetToOleShim(null, c);
        }

        private void PlotWindowPane_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e) {
            // If the window gets below a certain minimum size, plot to the minimum size
            // and user will be able to use scrollbars to see the whole thing
            int width = Math.Max((int)e.NewSize.Width, MinWidth);
            int height = Math.Max((int)e.NewSize.Height, MinHeight);
            Microsoft.VisualStudio.R.Package.Plots.PlotContentProvider.DoNotWait(PlotContentProvider.ResizePlotAsync(width, height));
        }

        public override void OnToolWindowCreated() {
            base.OnToolWindowCreated();

            IVsWindowFrame frame = this.Frame as IVsWindowFrame;
            frame.SetProperty((int)__VSFPROPID.VSFPROPID_ViewHelper, this);
        }

        public IPlotContentProvider PlotContentProvider { get; private set; }

        private IEnumerable<ICommand> GetCommands() {
            List<ICommand> commands = new List<ICommand>();

            _copyAndExportCommands = new PlotWindowCommand[] {
                new ExportPlotAsImageCommand(this),
                new ExportPlotAsPdfCommand(this),
                new CopyPlotAsBitmapCommand(this),
                new CopyPlotAsMetafileCommand(this),
            };
            _historyNextPlotCommand = new HistoryNextPlotCommand(this);
            _historyPreviousPlotCommand = new HistoryPreviousPlotCommand(this);

            commands.AddRange(_copyAndExportCommands);
            commands.Add(_historyNextPlotCommand);
            commands.Add(_historyPreviousPlotCommand);

            return commands;
        }

        private async System.Threading.Tasks.Task RefreshHistoryInfo() {
            var info = await PlotContentProvider.GetHistoryInfoAsync();
            SetHistoryInfo(info.ActivePlotIndex, info.PlotCount);
        }

        private void ClearHistoryInfo() {
            SetHistoryInfo(-1, 0);
        }

        private void SetHistoryInfo(int activeIndex, int plotCount) {
            if (activeIndex >= 0) {
                if (activeIndex < (plotCount - 1)) {
                    _historyNextPlotCommand.Enable();
                } else {
                    _historyNextPlotCommand.Disable();
                }
                if (activeIndex > 0) {
                    _historyPreviousPlotCommand.Enable();
                } else {
                    _historyPreviousPlotCommand.Disable();
                }

                foreach (var cmd in _copyAndExportCommands) {
                    cmd.Enable();
                }
            } else {
                _historyNextPlotCommand.Disable();
                _historyPreviousPlotCommand.Disable();

                foreach (var cmd in _copyAndExportCommands) {
                    cmd.Disable();
                }
            }

            IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            shell.UpdateCommandUI(1);
        }

        private void ContentProvider_PlotChanged(object sender, PlotChangedEventArgs e) {
            if (e.NewPlotElement == null) {
                ClearHistoryInfo();
            } else {
                Microsoft.VisualStudio.R.Package.Plots.PlotContentProvider.DoNotWait(RefreshHistoryInfo());
            }
        }

        internal void ExportPlotAsImage() {
            string destinationFilePath = VsAppShell.Current.BrowseForFileSave(IntPtr.Zero, Resources.PlotExportAsImageFilter, null, Resources.ExportPlotAsImageDialogTitle);
            if (!string.IsNullOrEmpty(destinationFilePath)) {
                string device = String.Empty;
                string extension = Path.GetExtension(destinationFilePath).TrimStart('.').ToLowerInvariant();
                switch (extension) {
                    case "png":
                        device = "png";
                        break;
                    case "bmp":
                        device = "bmp";
                        break;
                    case "tif":
                    case "tiff":
                        device = "tiff";
                        break;
                    case "jpg":
                    case "jpeg":
                        device = "jpeg";
                        break;
                    default:
                        VsAppShell.Current.ShowErrorMessage(string.Format(Resources.PlotExportUnsupportedImageFormat, extension));
                        return;
                }

                PlotContentProvider.ExportAsImage(destinationFilePath, device);
            }
        }

        internal void ExportPlotAsPdf() {
            string destinationFilePath = VsAppShell.Current.BrowseForFileSave(IntPtr.Zero, Resources.PlotExportAsPdfFilter, null, Resources.ExportPlotAsPdfDialogTitle);
            if (!string.IsNullOrEmpty(destinationFilePath)) {
                PlotContentProvider.ExportAsPdf(destinationFilePath);
            }
        }

        internal void NextPlot() {
            Microsoft.VisualStudio.R.Package.Plots.PlotContentProvider.DoNotWait(PlotContentProvider.NextPlotAsync());
        }

        internal void PreviousPlot() {
            Microsoft.VisualStudio.R.Package.Plots.PlotContentProvider.DoNotWait(PlotContentProvider.PreviousPlotAsync());
        }

        protected override void Dispose(bool disposing) {
            if (PlotContentProvider != null) {
                PlotContentProvider.PlotChanged -= ContentProvider_PlotChanged;
                PlotContentProvider.Dispose();
                PlotContentProvider = null;
            }

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
