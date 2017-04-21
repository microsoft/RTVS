// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Fixtures;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit;

namespace Microsoft.R.Host.Client.Test.Session {
    public partial class RSessionTest {
        public class Output : IAsyncLifetime {
            private readonly IBrokerClient _brokerClient;
            private readonly RSession _session;

            public Output(IServiceContainer services, TestMethodFixture testMethod) {
                _brokerClient = CreateLocalBrokerClient(services, nameof(RSessionTest) + nameof(Output));
                _session = new RSession(0, testMethod.FileSystemSafeName, _brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => { });
            }

            public async Task InitializeAsync() {
                await _session.StartHostAsync(new RHostStartupInfo(), null, 50000);
            }

            public async Task DisposeAsync() {
                await _session.StopHostAsync();
                _session.Dispose();
                _brokerClient.Dispose();
            }

            [Test]
            [Category.R.Session]
            public async Task UnicodeOutput() {
                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("Sys.setlocale('LC_CTYPE', 'Japanese_Japan.932')\n");
                }

                var output = new StringBuilder();
                _session.Output += (sender, e) => output.Append(e.Message);

                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("'日本語'\n");
                }

                output.ToString().Should().Be("[1] \"日本語\"\n");
            }
        }
    }
}
