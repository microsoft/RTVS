// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Stubs.Shell;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client.Host;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Components.Test.ConnectionManager {
    [ExcludeFromCodeCoverage]
    [Category.Connections]
    public sealed class ConnectionManagerTest {
        private readonly IServiceContainer _services;

        public ConnectionManagerTest(IServiceContainer services) {
            _services = services;
        }

        [Test]
        public async Task DisconnectAsync() {
            var connection = _services.GetService<IRSettings>().LastActiveConnection;

            var workflow = _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
            var connectionManager = workflow.Connections;
            await connectionManager.ConnectAsync(connection).Should().BeCompletedAsync();
            await connectionManager.DisconnectAsync().Should().BeCompletedAsync();

            workflow.RSessions.HasBroker.Should().BeFalse();
            workflow.RSession.IsHostRunning.Should().BeFalse();
        }

        [Test]
        public async Task DisconnectAsync_Canceled() {
            var connection = _services.GetService<IRSettings>().LastActiveConnection;

            var connectionManager = _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate().Connections;
            await connectionManager.ConnectAsync(connection).Should().BeCompletedAsync();

            var cts = new CancellationTokenSource();
            var disconnectAsyncTask = connectionManager.DisconnectAsync(cts.Token);
            cts.Cancel();
            await disconnectAsyncTask.Should().BeCanceledAsync();
        }

        [Test]
        public void RecentConnections() {
            var settings = _services.GetService<IRSettings>();
            var dateTime = DateTime.Now;
            settings.Connections = new[] {
                new ConnectionInfo("A", "http://127.0.0.1", null, true) { LastUsed = dateTime.AddHours(1) },
                new ConnectionInfo("B", "http://127.0.0.2", null, true) { LastUsed = dateTime.AddHours(-1) },
                new ConnectionInfo("C", "http://127.0.0.3", null, true) { LastUsed = dateTime.AddHours(2) },
                new ConnectionInfo("D", "http://127.0.0.4", null, true) { LastUsed = dateTime }
            };

            var cm = _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate().Connections;
            cm.RecentConnections.Should().StartWith(new [] { "C", "A", "D", "B" }, (ci, s) => ci.Name == s);
        }

        [Test]
        public async Task RecentConnections_AfterSwitch() {
            var settings = _services.GetService<IRSettings>();
            var dateTime = DateTime.Now;
            settings.Connections = new[] {
                new ConnectionInfo("A", "http://127.0.0.1", null, true) { LastUsed = dateTime.AddHours(-4) },
                new ConnectionInfo("B", "http://127.0.0.2", null, true) { LastUsed = dateTime.AddHours(-3) },
                new ConnectionInfo("C", "http://127.0.0.3", null, true) { LastUsed = dateTime.AddHours(-2) },
                new ConnectionInfo("D", "http://127.0.0.4", null, true) { LastUsed = dateTime.AddHours(-1) }
            };

            var cm = _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate().Connections;
            var connection = cm.RecentConnections.First(c => !c.IsRemote);
            await cm.ConnectAsync(connection).Should().BeCompletedAsync();

            cm.RecentConnections.Should().StartWith(connection);
            cm.ActiveConnection.Should().Be(connection);
        }

        [Test]
        public async Task RecentConnections_AfterSwitch_AddNewConnection() {
            var settings = _services.GetService<IRSettings>();
            var dateTime = DateTime.Now;
            settings.Connections = new[] {
                new ConnectionInfo("A", "http://127.0.0.1", null, true) { LastUsed = dateTime.AddHours(-4) },
                new ConnectionInfo("B", "http://127.0.0.2", null, true) { LastUsed = dateTime.AddHours(-3) },
                new ConnectionInfo("C", "http://127.0.0.3", null, true) { LastUsed = dateTime.AddHours(-2) }
            };

            var cm = _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate().Connections;
            var connection = cm.RecentConnections.First(c => !c.IsRemote);
            await cm.ConnectAsync(connection).Should().BeCompletedAsync();
            cm.GetOrAddConnection("D", "http://127.0.0.4", null, false);

            cm.RecentConnections.Should().StartWith(connection);
            cm.ActiveConnection.Should().Be(connection);
        }

        [Test]
        public async Task TryConnectToPreviouslyUsedAsync() {
            var connectionManager = _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate().Connections;
            await connectionManager.TryConnectToPreviouslyUsedAsync().Should().HaveResultAsync(true);
        }

        [Test]
        public async Task TryConnectToPreviouslyUsedAsync_AfterConnectAsyncFailed() {
            var unreachableConnection = new ConnectionInfo("A", "http://127.0.0.1", null, true);
            var settings = _services.GetService<IRSettings>();
            settings.Connections = new[] {
                unreachableConnection,
                settings.LastActiveConnection
            };

            using (var workflow = _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate()) {
                var security = (SecurityServiceStub)workflow.Shell.Security();
                security.GetUserCredentialsHandler = delegate { throw new RHostDisconnectedException(); };

                var connectionManager = workflow.Connections;
                await connectionManager.ConnectAsync(unreachableConnection).Should().BeCompletedAsync();

                await connectionManager.TryConnectToPreviouslyUsedAsync().Should().HaveResultAsync(false);

                await UIThreadHelper.Instance.TaskScheduler;
            }
        }
    }
}