// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Stubs;
using Microsoft.R.Interpreters;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;

namespace Microsoft.R.Host.Client.Test.Session {
    public partial class RSessionTest : IDisposable {
        private readonly IServiceContainer _services;
        private readonly TestMethodFixture _testMethod;
        private readonly IBrokerClient _brokerClient;

        public RSessionTest(IServiceContainer services, TestMethodFixture testMethod) {
            _services = services;
            _testMethod = testMethod;
            _brokerClient = CreateLocalBrokerClient(services, nameof(RSessionTest));
        }

        public void Dispose() {
            _brokerClient.Dispose();
        }

        [Test]
        [Category.R.Session]
        public void Lifecycle() {
            var disposed = false;

            var session = new RSession(0, _testMethod.FileSystemSafeName, _brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => disposed = true);
            disposed.Should().BeFalse();

            session.MonitorEvents();

            session.Dispose();
            session.ShouldRaise("Disposed");
            disposed.Should().BeTrue();
        }

        [Test]
        [Category.R.Session]
        public void Lifecycle_DoubleDispose() {
            var disposed = false;

            var session = new RSession(0, _testMethod.FileSystemSafeName, _brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => disposed = true);
            session.Dispose();

            disposed = false;
            session.MonitorEvents();
            session.Dispose();

            session.ShouldNotRaise("Disposed");
            disposed.Should().BeFalse();
        }

        [Test]
        [Category.R.Session]
        public async Task StartStop() {
            var session = new RSession(0, _testMethod.FileSystemSafeName, _brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => { });

            session.HostStarted.Should().NotBeCompleted();
            session.IsHostRunning.Should().BeFalse();

            await session.StartHostAsync(new RHostStartupInfo(), null, 50000);

            session.HostStarted.Should().BeRanToCompletion();
            session.IsHostRunning.Should().BeTrue();

            await session.StopHostAsync();

            session.HostStarted.Should().NotBeCompleted();
            session.IsHostRunning.Should().BeFalse();

            await session.StartHostAsync(new RHostStartupInfo(), null, 50000);

            session.HostStarted.Should().BeRanToCompletion();
            session.IsHostRunning.Should().BeTrue();
        }

        [Test]
        [Category.R.Session]
        public async Task Start_KillProcess_Start() {
            var session = new RSession(0, _testMethod.FileSystemSafeName, _brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => { });

            session.HostStarted.Should().NotBeCompleted();
            session.IsHostRunning.Should().BeFalse();
            
            await session.StartHostAsync(new RHostStartupInfo(), null, 50000);

            session.HostStarted.Should().BeRanToCompletion();
            session.IsHostRunning.Should().BeTrue();

            var sessionDisconnectedTask = EventTaskSources.IRSession.Disconnected.Create(session);
            var processId = await GetRSessionProcessId(session);
            Process.GetProcessById(processId).Kill();
            await sessionDisconnectedTask;

            session.IsHostRunning.Should().BeFalse();

            await session.StartHostAsync(new RHostStartupInfo(), null, 50000);

            session.HostStarted.Should().BeRanToCompletion();
            session.IsHostRunning.Should().BeTrue();
        }

        [Test]
        [Category.R.Session]
        public async Task EnsureStarted_KillProcess_EnsureStarted() {
            var session = new RSession(0, _testMethod.FileSystemSafeName, _brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => { });

            session.HostStarted.Should().NotBeCompleted();
            session.IsHostRunning.Should().BeFalse();

            await session.EnsureHostStartedAsync(new RHostStartupInfo(), null, 50000);

            session.HostStarted.Should().BeRanToCompletion();
            session.IsHostRunning.Should().BeTrue();

            var sessionDisconnectedTask = EventTaskSources.IRSession.Disconnected.Create(session);
            var processId = await GetRSessionProcessId(session);
            Process.GetProcessById(processId).Kill();
            await sessionDisconnectedTask;

            session.IsHostRunning.Should().BeFalse();

            await session.EnsureHostStartedAsync(new RHostStartupInfo(), null, 50000);

            session.HostStarted.Should().BeRanToCompletion();
            session.IsHostRunning.Should().BeTrue();
        }

        [Test]
        [Category.R.Session]
        public async Task DoubleStart() {
            var session = new RSession(0, _testMethod.FileSystemSafeName, _brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => { });
            Func<Task> start = () => session.StartHostAsync(new RHostStartupInfo(), null, 50000);

            var tasks = await ParallelTools.InvokeAsync(4, i => start(), 50000);
            tasks.Should().ContainSingle(t => t.Status == TaskStatus.RanToCompletion);

            await session.HostStarted;
            session.IsHostRunning.Should().BeTrue();

            await session.StopHostAsync();
            session.IsHostRunning.Should().BeFalse();
        }

        [Test]
        [Category.R.Session]
        public async Task StartStopMultipleSessions() {
            Func<int, Task<RSession>> start = async i => {
                var session = new RSession(i, _testMethod.FileSystemSafeName + i, _brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => { });
                await session.StartHostAsync(new RHostStartupInfo(), null, 50000);
                return session;
            };

            var sessionsTasks = await ParallelTools.InvokeAsync(4, start, 50000);

            sessionsTasks.Should().OnlyContain(t => t.Status == TaskStatus.RanToCompletion);
            var sessions = sessionsTasks.Select(t => t.Result).ToList();
            sessions.Should().OnlyContain(t => t.IsHostRunning);

            var sessionStopTasks = await ParallelTools.InvokeAsync(4, i => sessionsTasks[i].Result.StopHostAsync());
            sessionStopTasks.Should().OnlyContain(t => t.Status == TaskStatus.RanToCompletion);
            sessions.Should().OnlyContain(t => !t.IsHostRunning);
        }

        [Test]
        [Category.R.Session]
        public void StartRHostMissing() {
            var brokerClient = new LocalBrokerClient(nameof(RSessionTest), BrokerConnectionInfo.Create(null, "C", @"C:\", null, true), _services, new NullConsole(), Environment.SystemDirectory);
            var session = new RSession(0, _testMethod.FileSystemSafeName, brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => { });
            Func<Task> start = () => session.StartHostAsync(new RHostStartupInfo(), null, 10000);

            start.ShouldThrow<ComponentBinaryMissingException>();
        }

        [Test]
        [Category.R.Session]
        public async Task StopBeforeInitialized() {
            var session = new RSession(0, _testMethod.FileSystemSafeName, _brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => { });
            Func<Task> start = () => session.StartHostAsync(new RHostStartupInfo(), null, 10000);
            var startTask = Task.Run(start);

            await session.StopHostAsync();
            session.IsHostRunning.Should().BeFalse();

            await startTask;
        }

        [Test]
        [Category.R.Session]
        public async Task StopBeforeInitialized_RHostMissing() {
            var brokerClient = new LocalBrokerClient(nameof(RSessionTest), BrokerConnectionInfo.Create(null, "C", @"C:\", null, true), _services, new NullConsole(), Environment.SystemDirectory);
            var session = new RSession(0, _testMethod.FileSystemSafeName, brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => { });
            Func<Task> start = () => session.StartHostAsync(new RHostStartupInfo (), null, 10000);
            var startTask = Task.Run(start).SilenceException<RHostBinaryMissingException>();

            await session.StopHostAsync();
            session.IsHostRunning.Should().BeFalse();

            await startTask;
        }
        
        [Test]
        [Category.R.Session]
        public async Task StopReentrantLoop() {
            var callback = new RSessionCallbackStub();
            var session = new RSession(0, _testMethod.FileSystemSafeName, _brokerClient, new AsyncReaderWriterLock().CreateExclusiveReaderLock(), () => { });

            await session.StartHostAsync(new RHostStartupInfo(), callback, 50000);

            var testMrs = new AsyncManualResetEvent();
            callback.PlotHandler = (message, ct) => {
                testMrs.Set();
                return session.EvaluateAsync("x <- 1\n");
            };

            Task responceTask;
            using (var interaction = await session.BeginInteractionAsync()) {
                responceTask = interaction.RespondAsync("plot(1)\n");
            }

            await testMrs.WaitAsync().Should().BeCompletedAsync();

            await session.StopHostAsync().Should().BeCompletedAsync(20000);
            session.IsHostRunning.Should().BeFalse();

            await responceTask.Should().BeCanceledAsync();
        }


        private static IBrokerClient CreateLocalBrokerClient(IServiceContainer services, string name) 
            => new LocalBrokerClient(name, 
                BrokerConnectionInfo.Create(null, "Test", new RInstallation().GetCompatibleEngines().FirstOrDefault()?.InstallPath, null, true),
                services, 
                new NullConsole());

        private static Task<int> GetRSessionProcessId(IRSession session) 
            => session.EvaluateAsync<int>("Sys.getpid()", REvaluationKind.Normal);
    }
}
