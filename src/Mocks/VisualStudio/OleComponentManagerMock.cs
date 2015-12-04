using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class OleComponentManagerMock : IOleComponentManager {
        public int FContinueIdle() {
            return VSConstants.S_OK;
        }

        public int FCreateSubComponentManager(object piunkOuter, object piunkServProv, ref Guid riid, out IntPtr ppvObj) {
            throw new NotImplementedException();
        }

        public int FGetActiveComponent(uint dwgac, out IOleComponent ppic, OLECRINFO[] pcrinfo, uint dwReserved) {
            throw new NotImplementedException();
        }

        public int FGetParentComponentManager(out IOleComponentManager ppicm) {
            throw new NotImplementedException();
        }

        public int FInState(uint uStateID, IntPtr pvoid) {
            throw new NotImplementedException();
        }

        public int FOnComponentActivate(uint dwComponentID) {
            return VSConstants.S_OK;
        }

        public int FOnComponentExitState(uint dwComponentID, uint uStateID, uint uContext, uint cpicmExclude, IOleComponentManager[] rgpicmExclude) {
            return VSConstants.S_OK;
        }

        public int FPushMessageLoop(uint dwComponentID, uint uReason, IntPtr pvLoopData) {
            return VSConstants.S_OK;
        }

        public int FRegisterComponent(IOleComponent piComponent, OLECRINFO[] pcrinfo, out uint pdwComponentID) {
            pdwComponentID = 1;
            return VSConstants.S_OK;
        }

        public int FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam) {
            throw new NotImplementedException();
        }

        public int FRevokeComponent(uint dwComponentID) {
            throw new NotImplementedException();
        }

        public int FSetTrackingComponent(uint dwComponentID, int fTrack) {
            throw new NotImplementedException();
        }

        public int FUpdateComponentRegistration(uint dwComponentID, OLECRINFO[] pcrinfo) {
            throw new NotImplementedException();
        }

        public void OnComponentEnterState(uint dwComponentID, uint uStateID, uint uContext, uint cpicmExclude, IOleComponentManager[] rgpicmExclude, uint dwReserved) {
            throw new NotImplementedException();
        }

        public void QueryService(ref Guid guidService, ref Guid iid, out IntPtr ppvObj) {
            throw new NotImplementedException();
        }
    }
}
