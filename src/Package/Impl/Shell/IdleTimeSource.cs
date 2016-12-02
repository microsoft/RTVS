// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public sealed class IdleTimeSource : IOleComponent, IDisposable {
        public event EventHandler<EventArgs> OnIdle;
        public event EventHandler<EventArgs> OnTerminateApp;

        private uint _componentID = 0;

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
        public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked) {
            return 0;
        }

        public int FDoIdle(uint grfidlef) {
            OnIdle?.Invoke(this, EventArgs.Empty);
            return 0;
        }

        public int FPreTranslateMessage(MSG[] pMsg) {
            return 0;
        }

        public int FQueryTerminate(int fPromptUser) {
            // Although this theoretically can be canceled, it never used in VS
            // since package QueryClose is the proper way of canceling the shutdown.
            OnTerminateApp?.Invoke(this, EventArgs.Empty);
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
        }

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
