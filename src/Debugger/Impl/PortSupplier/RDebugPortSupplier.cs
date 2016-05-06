// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger.PortSupplier {

    [ComVisible(true)]
    [Guid(DebuggerGuids.PortSupplierCLSIDString)]
    public partial class RDebugPortSupplier : IDebugPortSupplier2, IDebugPortSupplierDescription2 {
        public RDebugPortSupplier() {
        }

        public int AddPort(IDebugPortRequest2 pRequest, out IDebugPort2 ppPort) {
            ppPort = new DebugPort(this, pRequest);
            return VSConstants.S_OK;
        }

        public int CanAddPort() {
            return VSConstants.S_OK;
        }

        public int EnumPorts(out IEnumDebugPorts2 ppEnum) {
            ppEnum = null;
            return VSConstants.S_OK;
        }

        public int GetPort(ref Guid guidPort, out IDebugPort2 ppPort) {
            ppPort = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetPortSupplierId(out Guid pguidPortSupplier) {
            pguidPortSupplier = DebuggerGuids.PortSupplier;
            return VSConstants.S_OK;
        }

        public int GetPortSupplierName(out string pbstrName) {
            pbstrName = "R Interactive sessions";
            return VSConstants.S_OK;
        }

        public int RemovePort(IDebugPort2 pPort) {
            return VSConstants.S_OK;
        }

        public int GetDescription(enum_PORT_SUPPLIER_DESCRIPTION_FLAGS[] pdwFlags, out string pbstrText) {
            pbstrText = "Allows attaching to an R Interactive window to debug the code executed in it.";
            return VSConstants.S_OK;
        }
    }
}
