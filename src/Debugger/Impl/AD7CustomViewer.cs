// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger {
    [ComVisible(true)]
    [Guid(DebuggerGuids.CustomViewerString)]
    public class AD7CustomViewer : IDebugCustomViewer {
        public AD7CustomViewer() {
        }

        public int DisplayValue(IntPtr hwnd, uint dwID, object pHostServices, IDebugProperty3 pDebugProperty) {
            var property = pDebugProperty as AD7Property;
            if (property == null || property.StackFrame.Engine.GridViewProvider == null) {
                return VSConstants.E_FAIL;
            }

            property.StackFrame.Engine.GridViewProvider.ShowDataGrid(property.EvaluationResult.ToEnvironmentIndependentResult());
            return VSConstants.S_OK;
        }
    }
}
