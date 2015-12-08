using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.OLE.Interop;
using NSubstitute;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public static class OleComponentManagerMock {
        public static IOleComponentManager Create() {
            IOleComponentManager cm = Substitute.For<IOleComponentManager>();
            uint value;

            cm.FContinueIdle().ReturnsForAnyArgs(VSConstants.S_OK);
            cm.FOnComponentActivate(Arg.Any<uint>()).ReturnsForAnyArgs(VSConstants.S_OK);
            cm.FOnComponentExitState(Arg.Any<uint>(), Arg.Any<uint>(), Arg.Any<uint>(), Arg.Any<uint>(), Arg.Any<IOleComponentManager[]>()).ReturnsForAnyArgs(VSConstants.S_OK);
            cm.FPushMessageLoop(Arg.Any<uint>(), Arg.Any<uint>(), Arg.Any<IntPtr>()).ReturnsForAnyArgs(VSConstants.S_OK);
            cm.FRegisterComponent(Arg.Any<IOleComponent>(), Arg.Any<OLECRINFO[]>(), out value).ReturnsForAnyArgs(x => {
                x[2] = 1;
                return VSConstants.S_OK;
            });
            return cm;
        }
    }
}
