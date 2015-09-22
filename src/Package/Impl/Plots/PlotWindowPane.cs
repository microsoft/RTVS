using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Plots
{
    public class PlotWindowPane : ToolWindowPane, IOleCommandTarget
    {
        public PlotWindowPane()
        {
            SetContent();
        }


        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();

            SetToolBar();
        }

        private void SetContent()
        {
            var xamlPresenter = new XamlPresenter();

            xamlPresenter.LoadXaml(@"<TextBlock 
xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"">
Test test test
</TextBlock>");

            Content = xamlPresenter;
        }

        private void SetToolBar()
        {
            var frame = (IVsWindowFrame)Frame;
            object otbh;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID.VSFPROPID_ToolbarHost, out otbh));
            IVsToolWindowToolbarHost tbh = otbh as IVsToolWindowToolbarHost;
            Guid guidPlotMenuGroup = GuidList.PlotWindowGuid;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(tbh.AddToolbar(VSTWT_LOCATION.VSTWT_TOP, ref guidPlotMenuGroup, CommandIDs.menuIdPlotToolbar));
        }

        #region IOleCommandTarget

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == GuidList.PlotWindowGuid)
            {
                switch (nCmdID)
                {
                    case CommandIDs.cmdidOpenPlot:
                        // TODO: factor out as utility
                        return VSConstants.S_OK;
                }
            }

            var nextTarget = this.ToolBarCommandTarget;
            if (nextTarget != null)
            {
                return nextTarget.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }

            return VSConstants.E_FAIL;
        }

        #endregion
    }
}
