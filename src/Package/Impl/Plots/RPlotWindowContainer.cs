using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Plots {

    /// <summary>
    /// Parent of x64 RPlot window
    /// </summary>
    internal sealed class RPlotWindowContainer : UserControl, IVsWindowPane {
        public PlotWindowMenu Menu { get; private set; }

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
            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
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

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e) {
            SetBgColor();
        }

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

        protected override void WndProc(ref Message m) {
            if (m.Msg == NativeMethods.WM_ACTIVATE_PLOT) {
                if (m.WParam != IntPtr.Zero) {
                    if (_rPlotWindowHandle != IntPtr.Zero && m.WParam != _rPlotWindowHandle) {
                        DestroyChildPlot(IntPtr.Zero);
                    }
                    Menu = new PlotWindowMenu(RPlotWindowHandle, m.WParam);
                    ActivatePlotWindow();
                    DelaySizePlot(100);
                    NativeMethods.ShowWindow(_rPlotWindowHandle, NativeMethods.ShowWindowCommands.Show);
                }
            } else if (m.Msg == NativeMethods.WM_SIZE) {
                DelaySizePlot(50);
            } else if (m.Msg == NativeMethods.WM_CLOSE) {
                DestroyChildPlot(IntPtr.Zero);
            } else if (m.Msg == NativeMethods.WM_PARENTNOTIFY && (int)m.WParam == NativeMethods.WM_DESTROY) {
                DestroyChildPlot(m.LParam);
            }
            base.WndProc(ref m);
        }

        private void ActivatePlotWindow() {
            PlotWindowPane pane = ToolWindowUtilities.FindWindowPane<PlotWindowPane>(0);
            IVsWindowFrame frame = pane.Frame as IVsWindowFrame;
            int onScreen = 0;
            frame.IsOnScreen(out onScreen);
            if (onScreen == 0) {
                ToolWindowUtilities.ShowWindowPane<PlotWindowPane>(0, focus: false);
            }
        }

        private void DelaySizePlot(int timeout) {
            IdleTimeAction.Cancel(typeof(RPlotWindowContainer));
            IdleTimeAction.Create(() => SizeChildPlot(), timeout, typeof(RPlotWindowContainer));
        }

        private void SizeChildPlot() {
            IntPtr handle = RPlotWindowHandle;
            if (handle != IntPtr.Zero) {
                NativeMethods.MoveWindow(handle, 0, 0, this.Width, this.Height, bRepaint: true);
            }
        }

        private void DestroyChildPlot(IntPtr handle) {
            if (handle == IntPtr.Zero || handle == _rPlotWindowHandle) {
                if (_rPlotWindowHandle != IntPtr.Zero) {
                    NativeMethods.PostMessage(_rPlotWindowHandle, NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }
                _rPlotWindowHandle = IntPtr.Zero;
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                VSColorTheme.ThemeChanged -= VSColorTheme_ThemeChanged;
                DestroyChildPlot(IntPtr.Zero);
            }
            base.Dispose(disposing);
        }

        protected override void OnParentBackColorChanged(EventArgs e) {
            SetBgColor();
            base.OnParentBackColorChanged(e);
        }

        private void SetBgColor() {
            IVsUIShell2 uiShell = VsAppShell.Current.GetGlobalService<IVsUIShell2>(typeof(SVsUIShell));
            uint color;
            uiShell.GetVSSysColorEx((int)__VSSYSCOLOREX.VSCOLOR_TOOLWINDOW_BACKGROUND, out color);
            this.BackColor = ColorTranslator.FromWin32((int)color);
        }
    }
}
