// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Testing;
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
            private readonly IBrokerClient _brokerClient;
            private readonly RSession _session;
            private readonly RSessionCallbackStub _callback;

            public ReadInput(IServiceContainer services, TestMethodFixture testMethod) {
                _brokerClient = CreateLocalBrokerClient(services, nameof(RSessionTest) + nameof(ReadInput));
                _session = new RSession(0, testMethod.FileSystemSafeName, _brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => { });
                _callback = new RSessionCallbackStub();
            }

            public async Task InitializeAsync() {
                await _session.StartHostAsync(new RHostStartupInfo (isInteractive:true), _callback, 50000);
                TestEnvironment.Current.TryAddTaskToWait(_session.RHost.GetRHostRunTask());
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
                var responds = new ConcurrentQueue<int>();
                var input = new ConcurrentQueue<string>();
                var output = new ConcurrentQueue<string>();
                void OutputHandler(object o, ROutputEventArgs e) => output.Enqueue(e.Message);

                Task<string> InputHandler(string prompt, int maximumLength, CancellationToken ct) {
                    input.Enqueue(prompt);
                    return Task.FromResult($"{prompt}\n");
                }

                _callback.ReadUserInputHandler = InputHandler;
                _session.Output += OutputHandler;
                await ParallelTools.InvokeAsync(10, async i => {
                    using (var interaction = await _session.BeginInteractionAsync()) {
                        responds.Enqueue(i);
                        await interaction.RespondAsync($"readline('{i}')");
                    }
                }, 20000);
                _session.Output -= OutputHandler;

                responds.Should().BeEquivalentTo(Enumerable.Range(0, 10));
                input.Should().Equal(responds.Select(i => i.ToString()));
                output.Should().Contain(responds.Select(i => $" \"{i}\""));
            }
        }
    }
}
