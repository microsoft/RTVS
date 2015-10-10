/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger.Engine.PortSupplier {

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
            return VSConstants.S_OK; // S_OK = true, S_FALSE = false
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
