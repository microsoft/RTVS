using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Plots {
    [Guid(WindowGuid)]
    internal class PlotWindowPane : ToolWindowPane, IVsWindowFrameNotify3 {
        internal const string WindowGuid = "970AD71C-2B08-4093-8EA9-10840BC726A3";

        //private SavePlotCommand _saveCommand;
        private Lazy<RPlotWindowContainer> _plotWindowContainer = Lazy.Create(() => new RPlotWindowContainer());

        public PlotWindowPane() {
            Caption = Resources.PlotWindowCaption;

            // set content with presenter
            //PlotContentProvider = new PlotContentProvider();
            //PlotContentProvider.PlotChanged += ContentProvider_PlotChanged;
            //Content = new XamlPresenter(PlotContentProvider);

            // initialize toolbar
            this.ToolBar = new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.plotWindowToolBarId);
            //Controller c = new Controller();
            //c.AddCommandSet(GetCommands());
            this.ToolBarCommandTarget = new CommandTargetToOleShim(null, new PlotWindowCommandController(this));
        }

        public override void OnToolWindowCreated() {
            base.OnToolWindowCreated();

            IVsWindowFrame frame = this.Frame as IVsWindowFrame;
            frame.SetProperty((int)__VSFPROPID.VSFPROPID_ViewHelper, this);
        }

        public override object GetIVsWindowPane() {
            return _plotWindowContainer.Value;
        }

        //public IPlotContentProvider PlotContentProvider { get; private set; }

        private IEnumerable<ICommand> GetCommands() {
            List<ICommand> commands = new List<ICommand>();

            //commands.Add(new OpenPlotCommand(this));

            //_saveCommand = new SavePlotCommand(this);
            //commands.Add(_saveCommand);

            //commands.Add(new ExportPlotCommand(this));
            //commands.Add(new FixPlotCommand(this));
            //commands.Add(new CopyPlotCommand(this));
            //commands.Add(new PrintPlotCommand(this));
            //commands.Add(new ZoomInPlotCommand(this));
            //commands.Add(new ZoomOutPlotCommand(this));

            return commands;
        }

        //private void ContentProvider_PlotChanged(object sender, PlotChangedEventArgs e) {
        //    if (e.NewPlotElement == null) {
        //        _saveCommand.Disable();
        //    } else {
        //        _saveCommand.Enable();
        //    }
        //}

        //public void OpenPlot() {
        //    string filePath = GetLoadFilePath();
        //    if (!string.IsNullOrEmpty(filePath)) {
        //        try {
        //            PlotContentProvider.LoadFile(filePath);
        //        } catch (Exception ex) {
        //            EditorShell.Current.ShowErrorMessage(
        //                string.Format(CultureInfo.InvariantCulture, Resources.CannotOpenPlotFile, ex.Message));
        //        }
        //    }
        //}

        //public void SavePlot() {
        //    string destinationFilePath = GetSaveFilePath();
        //    if (!string.IsNullOrEmpty(destinationFilePath)) {
        //        PlotContentProvider.SaveFile(destinationFilePath);
        //    }
        //}

        private string GetLoadFilePath() {
            return EditorShell.Current.BrowseForFileOpen(IntPtr.Zero,
                Resources.PlotFileFilter,
                // TODO: open in current project folder if one is active
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\",
                Resources.OpenPlotDialogTitle);
        }

        private string GetSaveFilePath() {
            return EditorShell.Current.BrowseForFileSave(IntPtr.Zero, Resources.PlotFileFilter, null, Resources.SavePlotDialogTitle);
        }

        protected override void Dispose(bool disposing) {
            //if (PlotContentProvider != null) {
            //    PlotContentProvider.PlotChanged -= ContentProvider_PlotChanged;
            //    PlotContentProvider.Dispose();
            //    PlotContentProvider = null;
            //}

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
            IVsWindowPane pane = GetIVsWindowPane() as IVsWindowPane;
            if (pane != null) {
                pane.ClosePane();
            }
            return VSConstants.S_OK;
        }
        #endregion
    }
}
