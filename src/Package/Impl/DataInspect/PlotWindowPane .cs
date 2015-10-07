using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Controls
{
    [Guid(WindowGuid)]
    public class PlotWindowPane : ToolWindowPane
    {
        internal const string WindowGuid = "970AD71C-2B08-4093-8EA9-10840BC726A3";

        public PlotWindowPane()
        {
            Caption = "R Plot";
            Content = new XamlPresenter();

            InitializePresenter();

            this.ToolBar = new CommandID(RGuidList.PlotWindowGuid, CommandIDs.menuIdPlotToolbar);
            this.ToolBarCommandTarget = new PlotWindowCommandTarget(this);
        }

        private void InitializePresenter()
        {
            DisplayXaml(@"<TextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
Please open a file to show XAML file here.
</TextBlock>");
        }

        private void OpenPlotCommand()
        {
            DisplayXamlFile(GetFileName());
        }

        public void DisplayXamlFile(string filePath)
        {
            var presenter = this.Content as XamlPresenter;
            if (presenter != null)
            {
                presenter.LoadXamlFile(filePath);
            }
        }

        public void DisplayXaml(string xaml)
        {
            var presenter = this.Content as XamlPresenter;
            if (presenter != null)
            {
                presenter.LoadXaml(xaml);
            }
        }

        // TODO: factor out to utility. Copied code from PTVS, Dialogs.cs
        private string GetFileName()
        {
            return BrowseForFileOpen(
                IntPtr.Zero,
                "XAML Files (*.xaml)|*.xaml|All Files (*.*)|*.*",
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Choose XAML File");
        }

        static string BrowseForFileOpen(
            IntPtr owner,
            string filter,
            string initialPath = null,
            string title = null)
        {
            IVsUIShell uiShell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            if (uiShell != null)
            {
                if (owner == IntPtr.Zero)
                {
                    ErrorHandler.ThrowOnFailure(uiShell.GetDialogOwnerHwnd(out owner));
                }

                VSOPENFILENAMEW[] openInfo = new VSOPENFILENAMEW[1];
                openInfo[0].lStructSize = (uint)Marshal.SizeOf(typeof(VSOPENFILENAMEW));
                openInfo[0].pwzFilter = filter.Replace('|', '\0') + "\0";
                openInfo[0].hwndOwner = owner;
                openInfo[0].pwzDlgTitle = title;
                openInfo[0].nMaxFileName = 260;
                var pFileName = Marshal.AllocCoTaskMem(520);
                openInfo[0].pwzFileName = pFileName;
                openInfo[0].pwzInitialDir = Path.GetDirectoryName(initialPath);
                var nameArray = (Path.GetFileName(initialPath) + "\0").ToCharArray();
                Marshal.Copy(nameArray, 0, pFileName, nameArray.Length);
                try
                {
                    int hr = uiShell.GetOpenFileNameViaDlg(openInfo);
                    if (hr == VSConstants.OLE_E_PROMPTSAVECANCELLED)
                    {
                        return null;
                    }
                    ErrorHandler.ThrowOnFailure(hr);
                    return Marshal.PtrToStringAuto(openInfo[0].pwzFileName);
                }
                finally
                {
                    if (pFileName != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(pFileName);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// internal class to handle Command
        /// </summary>
        class PlotWindowCommandTarget : IOleCommandTarget
        {
            private readonly PlotWindowPane _owner;
            public PlotWindowCommandTarget(PlotWindowPane owner)
            {
                _owner = owner;
            }

            public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
            {
                if (pguidCmdGroup == RGuidList.PlotWindowGuid)
                {
                    switch (nCmdID)
                    {
                        case CommandIDs.cmdidOpenPlot:
                            _owner.OpenPlotCommand();
                            return VSConstants.S_OK;
                    }
                }

                throw new InvalidOperationException();
            }

            public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
            {
                if (pguidCmdGroup == RGuidList.PlotWindowGuid)
                {
                    for (int i = 0; i < cCmds; i++)
                    {
                        switch (prgCmds[i].cmdID)
                        {
                            case CommandIDs.cmdidOpenPlot:
                                prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                                return VSConstants.S_OK;
                        }
                    }
                }

                throw new InvalidOperationException();
            }
        }
    }
}
