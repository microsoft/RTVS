// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Host.Client.Host;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.VisualStudio.R.Package.Test {
    [ExcludeFromCodeCoverage]
    [AssemblyFixture]
    public sealed class BrokerFixture: IDisposable {
        public IRHostBrokerConnector BrokerConnector { get; }

        public BrokerFixture() {
            BrokerConnector = new RHostBrokerConnector();
            BrokerConnector.SwitchToLocalBroker(this.GetType().Name);
        }

        public void Dispose() {
            BrokerConnector.Dispose();
        }
    }
}
