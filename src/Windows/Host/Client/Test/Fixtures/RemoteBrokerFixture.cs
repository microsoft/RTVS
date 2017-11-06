// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Security;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Interpreters;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Host.Client.Test.Fixtures {
    public class RemoteBrokerFixture : IMethodFixtureFactory {
        private readonly BinaryAsyncLock _connectLock = new BinaryAsyncLock();
        private readonly string _logFolder;

        private volatile RemoteBrokerProcess _process;

        internal string Address => _process.Address;
        internal SecureString Password => _process.Password.ToSecureString();

        public RemoteBrokerFixture() {
            _logFolder = Path.Combine(DeployFilesFixture.TestFilesRoot, "Logs");
        }

        public IRemoteBroker Create(IServiceContainer services) => new RemoteBrokerMethodFixture(this, services);

        public async Task EnsureBrokerStartedAsync(string name, IServiceContainer services) {
            var lockToken = await _connectLock.WaitAsync();
            try {
                if (!lockToken.IsSet) {
                    _process = new RemoteBrokerProcess(name, _logFolder, services.FileSystem(), services.GetService<IRInstallationService>(), services.Process());
                    await _process.StartAsync(() => {
                        _process = null;
                        _connectLock.EnqueueReset();
                    });
                }
                lockToken.Set();
            } finally {
                lockToken.Reset();
            }
        }
    }
}