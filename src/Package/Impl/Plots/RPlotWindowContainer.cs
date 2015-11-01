using System;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Plots {

    /// <summary>
    /// Parent of x64 RPlot window
    /// </summary>
    class RPlotWindowContainer : UserControl, IVsWindowPane {

        private DateTime _lastActivationMessageTime = DateTime.Now;
        private bool _connectedToIdle;
        private bool _sized;

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

            // TODO: set watermark
            SetBgColor();

            this.Show();
            return VSConstants.S_OK;
        }

        public int GetDefaultSize(SIZE[] pSize) {
            return VSConstants.E_NOTIMPL;
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
                if (_rPlotWindowHandle == IntPtr.Zero) {
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
            if (sb.ToString() == "GraphApp") {
                _rStaticPlotHandle = hWnd;
                return false;
            }
            return true;
        }

        protected override void OnClientSizeChanged(EventArgs e) {
            SizeChildPlot(RPlotWindowHandle);
            base.OnClientSizeChanged(e);
        }

        protected override void WndProc(ref Message m) {
            if (m.Msg == NativeMethods.WM_ACTIVATE_PLOT) {
                ForceSizeChildPlot();
                _lastActivationMessageTime = DateTime.Now;
                if (!_connectedToIdle) {
                    _connectedToIdle = true;
                    EditorShell.Current.Idle += OnIdle;
                }
            } else if (m.Msg == NativeMethods.WM_CLOSE) {
                DestroyChildPlot();
            } 
            base.WndProc(ref m);
        }

        private void OnIdle(object sender, EventArgs e) {
            if ((DateTime.Now - _lastActivationMessageTime).TotalMilliseconds > 100) {
                EditorShell.Current.Idle -= OnIdle;
                _connectedToIdle = false;

                PlotWindowPane pane = ToolWindowUtilities.FindWindowPane<PlotWindowPane>(0);
                IVsWindowFrame frame = pane.Frame as IVsWindowFrame;
                int onScreen = 0;
                frame.IsOnScreen(out onScreen);
                if (onScreen == 0) {
                    ToolWindowUtilities.ShowWindowPane<PlotWindowPane>(0, focus: false);
                    SizeChildPlot(IntPtr.Zero);
                }
            }
        }

        private void SizeChildPlot(IntPtr handle) {
            handle = handle == IntPtr.Zero ? RPlotWindowHandle : handle;
            if (handle != IntPtr.Zero) {
                NativeMethods.MoveWindow(handle, 0, 0, this.Width, this.Height, bRepaint: true);
            }
        }

        private void ForceSizeChildPlot() {
            if (!_sized) {
                IntPtr handle = RPlotWindowHandle;
                if (handle != IntPtr.Zero) {
                    NativeMethods.MoveWindow(handle, 0, 0, this.Width - 1, this.Height - 1, bRepaint: true);
                    NativeMethods.MoveWindow(handle, 0, 0, this.Width, this.Height, bRepaint: true);
                    _sized = true;
                }
            }
        }

        private void DestroyChildPlot() {
            if (_rPlotWindowHandle != IntPtr.Zero) {
                NativeMethods.PostMessage(_rPlotWindowHandle, NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                _rPlotWindowHandle = IntPtr.Zero;
            }
        }

        protected override void Dispose(bool disposing) {
            DestroyChildPlot();
            base.Dispose(disposing);
        }

        protected override void OnParentBackColorChanged(EventArgs e) {
            SetBgColor();
            base.OnParentBackColorChanged(e);
        }

        private void SetBgColor() {
            IVsUIShell2 uiShell = AppShell.Current.GetGlobalService<IVsUIShell2>(typeof(SVsUIShell));
            uint color;
            uiShell.GetVSSysColorEx((int)__VSSYSCOLOREX.VSCOLOR_TOOLWINDOW_BACKGROUND, out color);
            this.BackColor = ColorTranslator.FromWin32((int)color);
        }
    }
}
