// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    public sealed class UIHostLocaleMock: IUIHostLocale {
        public int GetUILocale(out uint plcid) {
            plcid = 1033;
            return VSConstants.S_OK;
        }

        public int GetDialogFont(UIDLGLOGFONT[] pLOGFONT) => throw new NotImplementedException();
    }
}
