using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Plots {
    internal static class RPlotWindowHook {
        private static IntPtr _nextHook = IntPtr.Zero;
        private static IntPtr _hwndPlotWindow = IntPtr.Zero;
        private static bool _processing;
        private static NativeMethods.HookProc _hookProc = new NativeMethods.HookProc(RPlotHookProc);

        private static IntPtr RPlotHookProc(int code, IntPtr wParam, IntPtr lParam) {
            if (code == 5) {
                if (!_processing) {
                    _processing = true;
                    StringBuilder sb = new StringBuilder(512);
                    NativeMethods.GetClassName(wParam, sb, 512);
                    if (sb.ToString() == "GraphApp")
                        if (RPlotWindowContainerHandle != NativeMethods.GetParent(wParam)) {
                            RPlotWindowHandle = wParam;
                            NativeMethods.RECT rc = new NativeMethods.RECT();
                            NativeMethods.SetWindowLong(wParam, (int)NativeMethods.WindowLongFlags.GWL_STYLE, 0x40000000 /*WS_CHILD*/);
                            NativeMethods.SetWindowLong(wParam, (int)NativeMethods.WindowLongFlags.GWL_EXSTYLE, 0);
                            NativeMethods.SetMenu(wParam, IntPtr.Zero);
                            NativeMethods.SetParent(wParam, RPlotWindowContainerHandle);
                            NativeMethods.GetClientRect(RPlotWindowContainerHandle, out rc);
                            NativeMethods.SetWindowPos(wParam, IntPtr.Zero, 0, 0, rc.Right, rc.Bottom, NativeMethods.SetWindowPosFlags.ShowWindow | NativeMethods.SetWindowPosFlags.FrameChanged);
                        }
                }
                _processing = false;
            }

            return NativeMethods.CallNextHookEx(_nextHook, code, wParam, lParam);
        }

        public static void SetHook() {
            _nextHook = NativeMethods.SetWindowsHookEx(NativeMethods.HookType.WH_CBT, _hookProc, IntPtr.Zero, NativeMethods.GetCurrentThreadId());
            if(_nextHook == IntPtr.Zero) {
                uint error = NativeMethods.GetLastError();
            }
        }

        public static void RemoveHook() {
            if (_nextHook != IntPtr.Zero) {
                NativeMethods.UnhookWindowsHookEx(_nextHook);
                _nextHook = IntPtr.Zero;
            }
        }

        public static IntPtr RPlotWindowHandle { get; private set; }

        private static IntPtr RPlotWindowContainerHandle {
            get {
                if (_hwndPlotWindow == IntPtr.Zero) {
                    ToolWindowUtilities.ShowWindowPane<PlotWindowPane>(0, true);
                    PlotWindowPane pane = ToolWindowUtilities.FindWindowPane<PlotWindowPane>(0);

                    IVsWindowPane windowPane = pane.GetIVsWindowPane() as IVsWindowPane;
                    Debug.Assert(windowPane != null);

                    Control c = windowPane as Control;
                    Debug.Assert(c != null);
                    Debug.Assert(c.Handle != IntPtr.Zero);

                    _hwndPlotWindow = c.Handle;
                }

                return _hwndPlotWindow;
            }
        }
    }
}
