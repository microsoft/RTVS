using System;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Plots {

    /// <summary>
    /// Parent of x64 RPlot window
    /// </summary>
    class RPlotWindowContainer : UserControl, IVsWindowPane {
        #region IVsWindowPane
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
        #endregion

        private IntPtr _rPlotWindowHandle = IntPtr.Zero;
        public IntPtr RPlotWindowHandle {
            get {
                if(_rPlotWindowHandle == IntPtr.Zero) {
                    NativeMethods.EnumChildWindows(this.Handle, EnumChildWindowsProc, IntPtr.Zero);
                    _rPlotWindowHandle = _rStaticPlotHandle;
                }

                return _rPlotWindowHandle;
            }
        }

        private static IntPtr _rStaticPlotHandle = IntPtr.Zero;
        private static bool EnumChildWindowsProc(IntPtr hWnd, IntPtr lParam) {
            StringBuilder sb = new StringBuilder(512);
            NativeMethods.GetClassName(hWnd, sb, 512);
            if(sb.ToString() == "GraphApp") {
                _rStaticPlotHandle = hWnd;
                return false;
            }
            return true;
        }

        protected override void OnClientSizeChanged(EventArgs e) {
            IntPtr rPlotWindow = RPlotWindowHandle;
            if (rPlotWindow != IntPtr.Zero) {
                NativeMethods.MoveWindow(rPlotWindow, 0, 0, this.Width, this.Height, bRepaint: true);
            }

            base.OnClientSizeChanged(e);
        }
    }
}
