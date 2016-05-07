// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger {
    internal sealed class AD7Module : IDebugModule2, IDebugModule3 {
        int IDebugModule3.GetInfo(enum_MODULE_INFO_FIELDS dwFields, MODULE_INFO[] pinfo) {
            throw new NotImplementedException();
        }

        int IDebugModule2.GetInfo(enum_MODULE_INFO_FIELDS dwFields, MODULE_INFO[] pinfo) {
            throw new NotImplementedException();
        }

        int IDebugModule3.GetSymbolInfo(enum_SYMBOL_SEARCH_INFO_FIELDS dwFields, MODULE_SYMBOL_SEARCH_INFO[] pinfo) {
            throw new NotImplementedException();
        }

        int IDebugModule3.IsUserCode(out int pfUser) {
            throw new NotImplementedException();
        }

        int IDebugModule3.LoadSymbols() {
            throw new NotImplementedException();
        }

        int IDebugModule3.ReloadSymbols_Deprecated(string pszUrlToSymbols, out string pbstrDebugMessage) {
            throw new NotImplementedException();
        }

        int IDebugModule2.ReloadSymbols_Deprecated(string pszUrlToSymbols, out string pbstrDebugMessage) {
            throw new NotImplementedException();
        }

        int IDebugModule3.SetJustMyCodeState(int fIsUserCode) {
            throw new NotImplementedException();
        }
    }
}