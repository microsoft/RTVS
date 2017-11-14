// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class ConnectionPointMock : IConnectionPoint
    {
        private IConnectionPointContainer _container;

        public ConnectionPointMock(IConnectionPointContainer container)
        {
            _container = container;
        }

        public void Advise(object pUnkSink, out uint pdwCookie)
        {
            pdwCookie = 1;
        }

        public void EnumConnections(out IEnumConnections ppEnum)
        {
            ppEnum = null;
        }

        public void GetConnectionInterface(out Guid pIID)
        {
            pIID = Guid.Empty;
        }

        public void GetConnectionPointContainer(out IConnectionPointContainer ppCPC)
        {
            ppCPC = _container;
        }

        public void Unadvise(uint dwCookie)
        {
        }
    }
}
