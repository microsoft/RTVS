// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Security;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Stubs.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Interpreters;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Host.Client.Test.Fixtures {
    public class RemoteBrokerFixture : IMethodFixtureFactory<IRemoteBroker> {
        private readonly BinaryAsyncLock _connectLock = new BinaryAsyncLock();
        private readonly IFileSystem _fileSystem;
        private readonly RInstallation _installations;
        private readonly ProcessServices _processService;
        private readonly string _logFolder;

        private volatile RemoteBrokerProcess _process;

        public IRemoteBroker Dummy { get; } = new RemoteBrokerMethodFixture(null, null);

        internal string Address => _process.Address;
        internal SecureString Password => _process.Password.ToSecureString();

        public RemoteBrokerFixture() {
            _fileSystem = new WindowsFileSystem();
            _installations = new RInstallation();
            _processService = new ProcessServices();
            _logFolder = Path.Combine(DeployFilesFixture.TestFilesRoot, "Logs");
        }

        public RemoteBrokerMethodFixture Create(IServiceContainer services) => new RemoteBrokerMethodFixture(this, services);

        public async Task EnsureBrokerStartedAsync(string name) {
            var lockToken = await _connectLock.WaitAsync();
            try {
                if (!lockToken.IsSet) {
                    _process = new RemoteBrokerProcess(name, _logFolder, _fileSystem, _installations, _processService);
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