// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public sealed class IdleTimeSource : IOleComponent, IDisposable {
        public event EventHandler<EventArgs> Idle;
        public event EventHandler<EventArgs> ApplicationClosing;
        public event EventHandler<EventArgs> ApplicationStarted;

        private uint _componentID = 0;
        private bool _startupComplete;

        public IdleTimeSource() {
            var crinfo = new OLECRINFO[1];
            crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
            crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime | (uint)_OLECRF.olecrfNeedPeriodicIdleTime;
            crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal | (uint)_OLECADVF.olecadvfRedrawOff | (uint)_OLECADVF.olecadvfWarningsOff;
            crinfo[0].uIdleTimeInterval = 200;

            var oleComponentManager = RPackage.GetGlobalService(typeof(SOleComponentManager)) as IOleComponentManager;
            oleComponentManager.FRegisterComponent(this, crinfo, out _componentID);
        }

        #region IOleComponent Members
        public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked) => VSConstants.S_OK;

        public int FDoIdle(uint grfidlef) {
            if(!_startupComplete) {
                ApplicationStarted?.Invoke(this, EventArgs.Empty);
                _startupComplete = true;
            }

            Idle?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        public int FPreTranslateMessage(MSG[] pMsg) => 0;

        public int FQueryTerminate(int fPromptUser) {
            // Although this theoretically can be canceled, it never used in VS
            // since package QueryClose is the proper way of canceling the shutdown.
            ApplicationClosing?.Invoke(this, EventArgs.Empty);
            return 1;
        }

        public int FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam) => 0;
        public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved) => IntPtr.Zero;

        public void OnActivationChange(IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo, int fHostIsActivating, OLECHOSTINFO[] pchostinfo, uint dwReserved) { }
        public void OnAppActivate(int fActive, uint dwOtherThreadID) { }
        public void OnEnterState(uint uStateID, int fEnter) { }
        public void OnLoseActivation() { }
        public void Terminate() { }
        #endregion

        #region IDisposable Members
        public void Dispose() {
            if (_componentID != 0) {
                var oleComponentManager = ServiceProvider.GlobalProvider.GetService(typeof(SOleComponentManager)) as IOleComponentManager;
                if (oleComponentManager != null)
                    oleComponentManager.FRevokeComponent(_componentID);

                _componentID = 0;
            }
        }
        #endregion
    }
}
