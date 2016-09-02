// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;

namespace Microsoft.R.Host.Client.Test.Fixtures {
    [ExcludeFromCodeCoverage]
    // Fixture doesn't import itself. Use AssemblyFixtureImportAttribute
    public sealed class BrokerFixture: IDisposable {
        public IRHostBrokerConnector BrokerConnector { get; }
        public IRSessionProvider SessionProvider { get; }

        public BrokerFixture() {
            SessionProvider = new RSessionProvider();
            BrokerConnector = new RHostBrokerConnector();
            BrokerConnector.SwitchToLocalBroker(GetType().Name);
        }

        public void Dispose() {
            BrokerConnector.Dispose();
        }
    }
}
