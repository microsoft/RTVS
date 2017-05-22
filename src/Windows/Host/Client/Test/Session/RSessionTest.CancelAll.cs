// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Testing;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Stubs;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit;

namespace Microsoft.R.Host.Client.Test.Session {
    public partial class RSessionTest {
        public class CancelAll : IAsyncLifetime {
            private readonly IBrokerClient _brokerClient;
            private readonly RSession _session;
            private readonly RSessionCallbackStub _callback;

            public CancelAll(IServiceContainer services, TestMethodFixture testMethod) {
                _callback = new RSessionCallbackStub();
                _brokerClient = CreateLocalBrokerClient(services, nameof(RSessionTest) + nameof(CancelAll));
                _session = new RSession(0, testMethod.FileSystemSafeName, _brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => {});
            }

            public async Task InitializeAsync() {
                await _session.StartHostAsync(new RHostStartupInfo(isInteractive: true), _callback, 50000);
                TestEnvironment.Current.TryAddTaskToWait(_session.RHost.GetRHostRunTask());
            }

            public async Task DisposeAsync() {
                await _session.StopHostAsync().Should().BeCompletedAsync();
                _session.Dispose();
                _brokerClient.Dispose();
            }

            [Test]
            [Category.R.Session]
            public async Task CancelAllInParallel() {
                Task responceTask;
                using (var interaction = await _session.BeginInteractionAsync()) {
                    responceTask = interaction.RespondAsync("while(TRUE){}\n");
                }

                await ParallelTools.InvokeAsync(4, i => _session.CancelAllAsync());

                _session.IsHostRunning.Should().BeTrue();
                responceTask.Status.Should().Be(TaskStatus.Canceled);
            }

            [Test]
            [Category.R.Session]
            public async Task CancelAll_CancellableHangingLoop() {
                var testMrs = new AsyncManualResetEvent();
                var plotMrs = new AsyncManualResetEvent();
                _callback.PlotHandler = (message, ct) => {
                    testMrs.Set();
                    return plotMrs.WaitAsync(ct);
                };

                Task responceTask;
                using (var interaction = await _session.BeginInteractionAsync()) {
                    responceTask = interaction.RespondAsync("plot(1)\n");
                }

                await testMrs.WaitAsync().Should().BeCompletedAsync();

                await _session.CancelAllAsync().Should().BeCompletedAsync();

                _session.IsHostRunning.Should().BeTrue();
                responceTask.Should().BeCanceled();
            }

            [Test]
            [Category.R.Session]
            public async Task CancelAll_HangingLoop_Cancel() {
                var testMrs = new AsyncManualResetEvent();
                var plotMrs = new AsyncManualResetEvent();
                _callback.PlotHandler = (message, ct) => {
                    testMrs.Set();
                    // ct is ignored on purpose
                    return plotMrs.WaitAsync();
                };

                var cancelAllCts = new CancellationTokenSource();
                Task responceTask;
                using (var interaction = await _session.BeginInteractionAsync()) {
                    responceTask = interaction.RespondAsync("plot(1)\n");
                }

                await testMrs.WaitAsync().Should().BeCompletedAsync();
                var cancelAllAsyncTask = _session.CancelAllAsync(cancelAllCts.Token);

                await cancelAllAsyncTask.Should().NotBeCompletedAsync();

                cancelAllCts.Cancel();
                await cancelAllAsyncTask.Should().BeCanceledAsync();

                _session.IsHostRunning.Should().BeTrue();
                await responceTask.Should().BeCanceledAsync();
                plotMrs.Set();
            }
        }
    }
}
