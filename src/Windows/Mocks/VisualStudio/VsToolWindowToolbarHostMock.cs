// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class VsToolWindowToolbarHostMock : IVsToolWindowToolbarHost3 {
        public int AddToolbar(VSTWT_LOCATION dwLoc, ref Guid pguid, uint dwId) {
            return VSConstants.S_OK;
        }

        public int AddToolbar3(VSTWT_LOCATION dwLoc, ref Guid pguid, uint dwId, IDropTarget pDropTarget, IOleCommandTarget pCommandTarget) {
            return VSConstants.S_OK;
        }

        public int BorderChanged() {
            return VSConstants.S_OK;
        }

        public int Close(uint dwReserved) {
            return VSConstants.S_OK;
        }

        public int ForceUpdateUI() {
            return VSConstants.S_OK;
        }

        public int Hide(uint dwReserved) {
            return VSConstants.S_OK;
        }

        public int ProcessMouseActivation(IntPtr hwnd, uint msg, uint wp, int lp) {
            return VSConstants.S_OK;
        }

        public int ProcessMouseActivationModal(IntPtr hwnd, uint msg, uint wp, int lp, out int plResult) {
            plResult = 0;
            return VSConstants.S_OK;
        }

        public int Show(uint dwReserved) {
            return VSConstants.S_OK;
        }

        public int ShowHideToolbar(ref Guid pguid, uint dwId, int fShow) {
            return VSConstants.S_OK;
        }
    }
}
