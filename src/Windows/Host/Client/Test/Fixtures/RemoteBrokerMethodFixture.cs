// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Services;
using Microsoft.R.Host.Client.Host;
using Microsoft.UnitTests.Core.XUnit;
using Xunit.Sdk;

namespace Microsoft.R.Host.Client.Test.Fixtures {
    public class RemoteBrokerMethodFixture : IMethodFixture, IRemoteBroker {
        private readonly RemoteBrokerFixture _remoteBrokerFixture;
        private string _testName;

        public RemoteBrokerMethodFixture(RemoteBrokerFixture remoteBrokerFixture) {
            _remoteBrokerFixture = remoteBrokerFixture;
        }

        public Task InitializeAsync(ITestInput testInput, IMessageBus messageBus) {
            _testName = testInput.FileSytemSafeName;
            return _remoteBrokerFixture.EnsureBrokerStartedAsync(testInput.TestClass.Assembly.GetName().Name);
        }

        public Task DisposeAsync(RunSummary result, IMessageBus messageBus) {
            return Task.CompletedTask;
        }

        public Task<bool> ConnectAsync(IRSessionProvider sessionProvider) {
            var brokerConnectionInfo = BrokerConnectionInfo.Create(_remoteBrokerFixture.SecurityService, _testName, _remoteBrokerFixture.Address);
            return sessionProvider.TrySwitchBrokerAsync(_testName, brokerConnectionInfo, _remoteBrokerFixture.SecurityService);
        }
    }
}