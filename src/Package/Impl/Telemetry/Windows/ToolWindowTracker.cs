using System;
using System.Timers;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Telemetry.Windows {
    internal sealed class ToolWindowTracker: IVsDebuggerEvents, IDisposable {
        private Timer _timer = new Timer();
        private uint _debuggerEventCookie;
        private uint _reportCount;

        public ToolWindowTracker() {
            var debugger = VsAppShell.Current.GetGlobalService<IVsDebugger>(typeof(IVsDebugger));
            if (debugger != null) {
                debugger.AdviseDebuggerEvents(this, out _debuggerEventCookie);

                _timer.Interval = new TimeSpan(0, 0, 10).TotalMilliseconds;
                _timer.AutoReset = true;
                _timer.Elapsed += OnElapsed;
                _timer.Start();
            }
        }

        private void OnElapsed(object sender, ElapsedEventArgs e) {
            VsAppShell.Current.DispatchOnUIThread(() => {
                ReportWindowLayout();
            });
        }


        public int OnModeChange(DBGMODE dbgmodeNew) {
            if(dbgmodeNew == DBGMODE.DBGMODE_Run) {
                ReportWindowLayout();
            }
            return VSConstants.S_OK;
        }

        private void ReportWindowLayout() {
            if (_reportCount < 4) {
                RtvsTelemetry.Current.ReportWindowLayout(VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell)));
                _reportCount++;
                if (_reportCount > 4) {
                    _timer?.Stop();
                }
            }
        }

        public void Dispose() {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;

            if(_debuggerEventCookie != 0) {
                var debugger = VsAppShell.Current.GetGlobalService<IVsDebugger>(typeof(IVsDebugger));
                debugger.UnadviseDebuggerEvents(_debuggerEventCookie);
                _debuggerEventCookie = 0;
            }
        }
    }
}
