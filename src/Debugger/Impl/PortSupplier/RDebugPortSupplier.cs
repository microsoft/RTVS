// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.R.Debugger.PortSupplier {

    [ComVisible(true)]
    [Guid(DebuggerGuids.PortSupplierCLSIDString)]
    public partial class RDebugPortSupplier : IDebugPortSupplier2, IDebugPortSupplierDescription2 {
        public const string PortName = "Default";

        public RDebugPortSupplier() {
        }

        public int AddPort(IDebugPortRequest2 pRequest, out IDebugPort2 ppPort) {
            string name;
            Marshal.ThrowExceptionForHR(pRequest.GetPortName(out name));

            if (name != PortName) {
                ppPort = null;
                return VSConstants.E_INVALIDARG;
            }

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
            pbstrText = Resources.PortSupplierDescription;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Returns a string that is suitable for use with <see cref="VsDebugTargetInfo2.bstrExe"/> when attaching to an R process.
        /// </summary>
        /// <param name="processId">
        /// Process ID for the R session to attach to. This should be the same value as used for <see cref="VsDebugTargetInfo2.dwProcessId"/>.
        /// </param>
        /// <remarks>
        /// Both <see cref="VsDebugTargetInfo2.bstrExe"/> and <see cref="VsDebugTargetInfo2.dwProcessId"/> must be filled for attach to
        /// be successful, even though they contain duplicate data.
        /// </remarks>
        /// <seealso cref="GetProcessId"/>
        public static string GetExecutableForAttach(uint processId) =>
            (char)0 + "0x" + processId.ToString("X", CultureInfo.InvariantCulture);
    }
}
