// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class VsTextLinesMock : VsTextBufferMock, IVsTextLines, IObjectWithSite, IConnectionPointContainer
    {
        #region IObjectWithSite
        public void SetSite(object pUnkSite)
        {
        }
        #endregion

        #region IConnectionPointContainer
        public void EnumConnectionPoints(out IEnumConnectionPoints ppEnum)
        {
            ppEnum = null;
        }

        public void FindConnectionPoint(ref Guid riid, out IConnectionPoint ppCP)
        {
            ppCP = new ConnectionPointMock(this);
        }
        #endregion
    }
}
