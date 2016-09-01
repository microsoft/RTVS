// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit;

namespace Microsoft.R.ExecutionTracing.Test {
    [ExcludeFromCodeCoverage]
    public class DebugReplTest : IAsyncLifetime {
        private readonly MethodInfo _testMethod;
        private readonly IRHostBrokerConnector _brokerConnector;
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRSession _session;

        public DebugReplTest(TestMethodFixture testMethod) {
            _testMethod = testMethod.MethodInfo;
            _brokerConnector = new RHostBrokerConnector();
            _brokerConnector.SwitchToLocalBroker(nameof(DebugReplTest));
            _sessionProvider = new RSessionProvider();
            _session = _sessionProvider.GetOrCreate(Guid.NewGuid(), _brokerConnector);
        }

        public async Task InitializeAsync() {
            await _session.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name
            }, new RHostClientTestApp(), 50000);
        }

        public async Task DisposeAsync() {
            await _session.StopHostAsync();
            _sessionProvider.Dispose();
            _brokerConnector.Dispose();
        }

        [Test]
        [Category.R.ExecutionTracing]
        public async Task InteractDuringBrowse() {
            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile("x <- 'old'; browser()")) {
                var browse = new TaskCompletionSource<bool>();
                tracer.Browse += (s, e) => {
                    browse.TrySetResult(true);
                };

                await sf.Source(_session);
                await browse.Task;

                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("x <- 'new'\n");
                }

                var x = await _session.EvaluateAsync<string>("x", REvaluationKind.Normal);

                x.Should().Be("new");
            }
        }
    }
}
