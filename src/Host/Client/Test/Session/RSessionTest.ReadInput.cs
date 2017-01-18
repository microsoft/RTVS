// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Test.Fixtures;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Stubs;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit;

namespace Microsoft.R.Host.Client.Test.Session {
    public partial class RSessionTest {
        public class ReadInput : IAsyncLifetime {
            private readonly TaskObserverMethodFixture _taskObserver;
            private readonly IBrokerClient _brokerClient;
            private readonly RSession _session;
            private readonly RSessionCallbackStub _callback;

            public ReadInput(CoreServicesFixture coreServices, TestMethodFixture testMethod, TaskObserverMethodFixture taskObserver) {
                _taskObserver = taskObserver;
                _brokerClient = CreateLocalBrokerClient(coreServices, nameof(RSessionTest) + nameof(ReadInput));
                _session = new RSession(0, testMethod.FileSystemSafeName, _brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => { });
                _callback = new RSessionCallbackStub();
            }

            public async Task InitializeAsync() {
                await _session.StartHostAsync(new RHostStartupInfo (), _callback, 50000);

                _taskObserver.ObserveTaskFailure(_session.RHost.GetRHostRunTask());
            }

            public async Task DisposeAsync() {
                await _session.StopHostAsync();
                _session.Dispose();
                _brokerClient.Dispose();
            }

            [Test]
            public async Task Paste() {
                var input = @"
h <- 'Hello'
name <- readline('Name:')
paste(h, name)
";
                var output = new List<string>();
                _callback.ReadUserInputHandler = (m, l, c) => Task.FromResult("Goo\n");
                using (var interaction = await _session.BeginInteractionAsync()) {
                    _session.Output += (o, e) => output.Add(e.Message);
                    await interaction.RespondAsync(input);
                }

                string.Join("", output).Should().Be("[1] \"Hello Goo\"\n");
            }

            [Test]
            public async Task ConcurrentRequests() {
                var output = new List<string>();
                EventHandler<ROutputEventArgs> outputHandler = (o, e) => output.Add(e.Message);

                _callback.ReadUserInputHandler = (m, l, c) => Task.FromResult($"{m}\n");

                await ParallelTools.InvokeAsync(10, async i => {
                    using (var interaction = await _session.BeginInteractionAsync()) {
                        _session.Output += outputHandler;
                        await interaction.RespondAsync($"readline('{i}')");
                        _session.Output -= outputHandler;
                    }
                });

                output.Should().Contain(Enumerable.Range(0, 10).Select(i => $" \"{i}\""));
            }
        }
    }
}
