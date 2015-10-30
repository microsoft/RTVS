using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Plots {

    /// <summary>
    /// Parent of x64 RPlot window
    /// </summary>
    class RPlotWindowContainer : UserControl, IVsWindowPane {
 
        private IntPtr _rPlotWindowHandle = IntPtr.Zero;
        private Control _rPlotWindowControl;

        public int ClosePane() {
            return VSConstants.S_OK;
        }

        public int CreatePaneWindow(IntPtr hwndParent, int x, int y, int cx, int cy, out IntPtr hwnd) {

            this.Left = x;
            this.Top = y;
            this.Width = cx;
            this.Height = cy;

            if (this.Handle == null) {
                this.CreateHandle();
            }

            hwnd = this.Handle;
            NativeMethods.SetParent(this.Handle, hwndParent);
            this.Show();

            this.BackColor = System.Drawing.Color.Aqua;
            return VSConstants.S_OK;
        }

        public int GetDefaultSize(SIZE[] pSize) {
            pSize[0].cx = 200;
            pSize[0].cy = 400;
            return VSConstants.S_OK;
        }

        public int LoadViewState(IStream pStream) {
            return VSConstants.S_OK;
        }

        public int SaveViewState(IStream pStream) {
            return VSConstants.S_OK;
        }

        public int SetSite(OLE.Interop.IServiceProvider psp) {
            return VSConstants.S_OK;
        }

        public int TranslateAccelerator(OLE.Interop.MSG[] lpmsg) {
            return VSConstants.E_NOTIMPL;
        }

        protected override void OnClientSizeChanged(EventArgs e) {
            FindRPlotWindow();

            if (_rPlotWindowControl != null) {
                _rPlotWindowControl.Width = this.Width;
                _rPlotWindowControl.Height = this.Height;
            }

            base.OnClientSizeChanged(e);
        }

        private void FindRPlotWindow() {
            if (_rPlotWindowControl == null) {
                NativeMethods.EnumChildWindows(this.Handle, EnumChildWindowsProc, IntPtr.Zero);
            }
            _rPlotWindowControl = _rPlotWindowHandle != IntPtr.Zero ? Control.FromHandle(_rPlotWindowHandle) : null;
        }

        private bool EnumChildWindowsProc(IntPtr hWnd, IntPtr lParam) {
            StringBuilder sb = new StringBuilder(512);
            NativeMethods.GetClassName(hWnd, sb, 512);
            if (sb.ToString() == "GraphApp") {
                _rPlotWindowHandle = hWnd;
                return false;
            }

            return true;
        }
    }
}
