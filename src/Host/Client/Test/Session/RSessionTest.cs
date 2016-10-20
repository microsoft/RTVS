// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Interpreters;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;

namespace Microsoft.R.Host.Client.Test.Session {
    public partial class RSessionTest : IDisposable {
        private readonly MethodInfo _testMethod;
        private readonly IBrokerClient _brokerClient;

        public RSessionTest(TestMethodFixture testMethod) {
            _testMethod = testMethod.MethodInfo;
            _brokerClient = CreateLocalBrokerClient(nameof(RSessionTest));
        }

        public void Dispose() {
            _brokerClient.Dispose();
        }

        [Test]
        [Category.R.Session]
        public void Lifecycle() {
            var disposed = false;

            var session = new RSession(0, _brokerClient, () => disposed = true);
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

            var session = new RSession(0, _brokerClient, () => disposed = true);
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
            var session = new RSession(0, _brokerClient, () => { });

            session.HostStarted.Status.Should().Be(TaskStatus.WaitingForActivation);
            session.IsHostRunning.Should().BeFalse();
            
            await session.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name
            }, null, 50000);

            session.HostStarted.Status.Should().Be(TaskStatus.RanToCompletion);
            session.IsHostRunning.Should().BeTrue();

            await session.StopHostAsync();

            session.HostStarted.Status.Should().Be(TaskStatus.WaitingForActivation);
            session.IsHostRunning.Should().BeFalse();

            await session.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name
            }, null, 50000);

            session.HostStarted.Status.Should().Be(TaskStatus.RanToCompletion);
            session.IsHostRunning.Should().BeTrue();
        }

        [Test]
        [Category.R.Session]
        public async Task DoubleStart() {
            var session = new RSession(0, _brokerClient, () => { });
            Func<Task> start = () => session.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name
            }, null, 50000);

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
                var session = new RSession(i, _brokerClient, () => { });
                await session.StartHostAsync(new RHostStartupInfo { Name = _testMethod.Name + i }, null, 50000);
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
            var brokerClient = new LocalBrokerClient(nameof(RSessionTest), @"C:\", TestCoreServices.CreateReal(), new NullConsole(), Environment.SystemDirectory);
            var session = new RSession(0, brokerClient, () => { });
            Func<Task> start = () => session.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name
            }, null, 10000);

            start.ShouldThrow<RHostBrokerBinaryMissingException>();
        }

        [Test]
        [Category.R.Session]
        public async Task StopBeforeInitialized() {
            var session = new RSession(0, _brokerClient, () => { });
            Func<Task> start = () => session.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name
            }, null, 10000);
            var startTask = Task.Run(start);

            await session.StopHostAsync();
            session.IsHostRunning.Should().BeFalse();

            await startTask;
        }

        [Test]
        [Category.R.Session]
        public async Task StopBeforeInitialized_RHostMissing() {
            var brokerClient = new LocalBrokerClient(nameof(RSessionTest), @"C:\",
                TestCoreServices.CreateReal(), new NullConsole(), Environment.SystemDirectory);
            var session = new RSession(0, brokerClient, () => { });
            Func<Task> start = () => session.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name
            }, null, 10000);
            var startTask = Task.Run(start).SilenceException<RHostBrokerBinaryMissingException>();

            await session.StopHostAsync();
            session.IsHostRunning.Should().BeFalse();

            await startTask;
        }

        private static IBrokerClient CreateLocalBrokerClient(string name) {
            return new LocalBrokerClient(name, new RInstallation().GetCompatibleEngines().FirstOrDefault()?.InstallPath, TestCoreServices.CreateReal(), new NullConsole());
        }
    }
}
