// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Interpreters;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;

namespace Microsoft.R.Host.Client.Test.Session {
    public partial class RSessionTest : IDisposable {
        private readonly MethodInfo _testMethod;
        private readonly IRHostConnector _hostConnector;

        public RSessionTest(TestMethodFixture testMethod) {
            _testMethod = testMethod.MethodInfo;
            _hostConnector = CreateLocalConnector(nameof(RSessionTest));
        }

        public void Dispose() {
            _hostConnector.Dispose();
        }

        [Test]
        [Category.R.Session]
        public void Lifecycle() {
            var disposed = false;

            var session = new RSession(0, _hostConnector, () => disposed = true);
            disposed.Should().BeFalse();

            session.MonitorEvents();

            session.Dispose();
            session.ShouldRaise("Disposed");
            disposed.Should().BeTrue();

            disposed = false;
            session.MonitorEvents();

            session.Dispose();
            session.ShouldNotRaise("Disposed");
            disposed.Should().BeFalse();
        }

        [Test]
        [Category.R.Session]
        public async Task StartStop() {
            var session = new RSession(0, _hostConnector, () => { });

            session.HostStarted.Status.Should().Be(TaskStatus.Canceled);
            session.IsHostRunning.Should().BeFalse();
            
            await session.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name
            }, null, 50000);

            session.HostStarted.Status.Should().Be(TaskStatus.RanToCompletion);
            session.IsHostRunning.Should().BeTrue();

            await session.StopHostAsync();

            session.HostStarted.Status.Should().Be(TaskStatus.Canceled);
            session.IsHostRunning.Should().BeFalse();
        }

        [Test]
        [Category.R.Session]
        public async Task DoubleStart() {
            var session = new RSession(0, _hostConnector, () => { });
            Func<Task> start = () => session.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name
            }, null, 50000);

            Func<Task> f = () => ParallelTools.InvokeAsync(4, i => start());
            f.ShouldThrow<InvalidOperationException>();

            await session.HostStarted;
            session.IsHostRunning.Should().BeTrue();

            await session.StopHostAsync();
            session.IsHostRunning.Should().BeFalse();
        }

        [Test]
        [Category.R.Session]
        public async Task StartStopMultipleSessions() {
            Func<int, Task<RSession>> start = async i => {
                var session = new RSession(i, _hostConnector, () => { });
                await session.StartHostAsync(new RHostStartupInfo { Name = _testMethod.Name + i }, null, 50000);
                return session;
            };

            var sessions = await ParallelTools.InvokeAsync(4, start);
            sessions.Should().OnlyContain(s => s.IsHostRunning);

            await ParallelTools.InvokeAsync(4, i => sessions[i].StopHostAsync());
            sessions.Should().OnlyContain(s => !s.IsHostRunning);
        }

        [Test]
        [Category.R.Session]
        public void StartRHostMissing() {
            var hostConnector = new LocalRHostConnector(nameof(RSessionTest), @"C:\", Environment.SystemDirectory);
            var session = new RSession(0, hostConnector, () => { });
            Func<Task> start = () => session.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name
            }, null, 10000);

            start.ShouldThrow<RHostBinaryMissingException>();
        }

        [Test(Skip="https://github.com/Microsoft/RTVS/issues/1196")]
        [Category.R.Session]
        public async Task StopBeforeInitialized() {
            var hostConnector = new LocalRHostConnector(nameof(RSessionTest), @"C:\", Environment.SystemDirectory);
            var session = new RSession(0, hostConnector, () => { });
            Func<Task> start = () => session.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name
            }, null, 10000);

            Task.Run(start).DoNotWait();
            await session.StopHostAsync();

            session.IsHostRunning.Should().BeFalse();
        }

        private static IRHostConnector CreateLocalConnector(string name) {
            return new LocalRHostConnector(name, new RInstallation().GetCompatibleEngines().FirstOrDefault()?.InstallPath);
        }
    }
}
