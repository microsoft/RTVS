// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Stubs.Shell;
using Microsoft.R.Host.Client.Host;
using Microsoft.UnitTests.Core.XUnit;
using Xunit.Sdk;

namespace Microsoft.R.Host.Client.Test.Fixtures {
    public class RemoteBrokerMethodFixture : IMethodFixture, IRemoteBroker {
        private const string UserName = "test_name";
        private readonly RemoteBrokerFixture _remoteBrokerFixture;
        private readonly IServiceContainer _services;
        private string _testName;
        private string _assemblyName;

        public RemoteBrokerMethodFixture(RemoteBrokerFixture remoteBrokerFixture, IServiceContainer services) {
            _remoteBrokerFixture = remoteBrokerFixture;
            _services = services;
        }

        public Task InitializeAsync(ITestInput testInput, IMessageBus messageBus) {
            _testName = testInput.FileSytemSafeName;
            _assemblyName = testInput.TestClass.Assembly.GetName().Name;
            return _remoteBrokerFixture.EnsureBrokerStartedAsync(_assemblyName);
        }

        public Task DisposeAsync(RunSummary result, IMessageBus messageBus) {
            return Task.CompletedTask;
        }

        public async Task<bool> ConnectAsync(IRSessionProvider sessionProvider) {
            var securityService = _services.GetService<ISecurityService>();
            if (securityService is SecurityServiceStub securityServiceStub) { 
                securityServiceStub.GetUserNameHandler = s => UserName;
                securityServiceStub.GetUserCredentialsHandler = (authority, workspaceName) => Credentials.Create(UserName, _remoteBrokerFixture.Password);
            }

            await _remoteBrokerFixture.EnsureBrokerStartedAsync(_assemblyName);

            var brokerConnectionInfo = BrokerConnectionInfo.Create(securityService, _testName, _remoteBrokerFixture.Address, null, false);
            return await sessionProvider.TrySwitchBrokerAsync(_testName, brokerConnectionInfo);
        }
    }
}