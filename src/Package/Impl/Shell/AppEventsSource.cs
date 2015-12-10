using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public sealed class AppEventsSource : IOleComponent, IDisposable {
        public event EventHandler<EventArgs> OnIdle;
        public event EventHandler<EventArgs> OnTerminate;

        private uint _componentID = 0;

        public AppEventsSource() {
            OLECRINFO[] crinfo = new OLECRINFO[1];
            crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
            crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime | (uint)_OLECRF.olecrfNeedPeriodicIdleTime;
            crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal | (uint)_OLECADVF.olecadvfRedrawOff | (uint)_OLECADVF.olecadvfWarningsOff;
            crinfo[0].uIdleTimeInterval = 200;

            IOleComponentManager oleComponentManager = VsAppShell.Current.GetGlobalService<IOleComponentManager>(typeof(SOleComponentManager));
            int hr = oleComponentManager.FRegisterComponent(this, crinfo, out _componentID);
            Debug.Assert(ErrorHandler.Succeeded(hr));
        }

        #region IOleComponent Members
        public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked) {
            return 0;
        }

        public int FDoIdle(uint grfidlef) {
            if (OnIdle != null) {
                OnIdle(this, EventArgs.Empty);
            }

            return 0;
        }

        public int FPreTranslateMessage(MSG[] pMsg) {
            return 0;
        }

        public int FQueryTerminate(int fPromptUser) {
            return 1;
        }

        public int FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam) {
            return 0;
        }

        public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved) {
            return IntPtr.Zero;
        }

        public void OnActivationChange(IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo, int fHostIsActivating, OLECHOSTINFO[] pchostinfo, uint dwReserved) {
        }

        public void OnAppActivate(int fActive, uint dwOtherThreadID) {
        }

        public void OnEnterState(uint uStateID, int fEnter) {
        }

        public void OnLoseActivation() {
        }

        public void Terminate() {
            if (OnTerminate != null) {
                OnTerminate(this, EventArgs.Empty);
            }
        }

        #endregion

        #region IDisposable Members
        public void Dispose() {
            if (_componentID != 0) {
                var oleComponentManager = VsAppShell.Current.GetGlobalService<IOleComponentManager>(typeof(SOleComponentManager));

                if (oleComponentManager != null) {
                    int hr = oleComponentManager.FRevokeComponent(_componentID);
                    Debug.Assert(ErrorHandler.Succeeded(hr));
                }

                _componentID = 0;
            }
        }
        #endregion
    }
}
