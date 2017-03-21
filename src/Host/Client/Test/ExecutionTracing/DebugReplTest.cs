// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit;

namespace Microsoft.R.ExecutionTracing.Test {
    [ExcludeFromCodeCoverage]
    public class DebugReplTest : IAsyncLifetime {
        private readonly TestCoreShell _coreShell = new TestCoreShell(null);
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRSession _session;

        public DebugReplTest(TestMethodFixture testMethod) {
            _sessionProvider = new RSessionProvider(_coreShell.Services);
            _session = _sessionProvider.GetOrCreate(testMethod.FileSystemSafeName);
        }

        public async Task InitializeAsync() {
            await _sessionProvider.TrySwitchBrokerAsync(nameof(DebugReplTest));
            await _session.StartHostAsync(new RHostStartupInfo(), new RHostClientTestApp(), 50000);
        }

        public async Task DisposeAsync() {
            await _session.StopHostAsync();
            _sessionProvider.Dispose();
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
