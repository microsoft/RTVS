// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
            cm.FOnComponentActivate(0u).ReturnsForAnyArgs(VSConstants.S_OK);
            cm.FOnComponentExitState(0u, 0, 0u, 0u, null).ReturnsForAnyArgs(VSConstants.S_OK);
            cm.FPushMessageLoop(0u, 0u, IntPtr.Zero).ReturnsForAnyArgs(VSConstants.S_OK);
            cm.FRegisterComponent(null, null, out value).ReturnsForAnyArgs(x => {
                x[2] = 1;
                return VSConstants.S_OK;
            });
            return cm;
        }
    }
}
