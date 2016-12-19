// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Components.Test.ConnectionManager {
    [ExcludeFromCodeCoverage]
    [Category.Connections]
    public sealed class ConnectionManagerTest : IDisposable {
        private readonly IExportProvider _exportProvider;

        public ConnectionManagerTest(RComponentsMefCatalogFixture mefCatalogFixture) {
            _exportProvider = mefCatalogFixture.CreateExportProvider();
        }

        [Test]
        public void RecentConnections() {
            var settings = _exportProvider.GetExportedValue<IRSettings>();
            var dateTime = DateTime.Now;
            settings.Connections = new[] {
                new ConnectionInfo("A", "http://127.0.0.1", null, true) { LastUsed = dateTime.AddHours(1) },
                new ConnectionInfo("B", "http://127.0.0.2", null, true) { LastUsed = dateTime.AddHours(-1) },
                new ConnectionInfo("C", "http://127.0.0.3", null, true) { LastUsed = dateTime.AddHours(2) },
                new ConnectionInfo("D", "http://127.0.0.4", null, true) { LastUsed = dateTime }
            };

            var cm = _exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate().Connections;
            cm.RecentConnections.Should().StartWith(new [] { "C", "A", "D", "B" }, (ci, s) => ci.Name == s);
        }

        [Test]
        public async Task RecentConnections_AfterSwitch() {
            var settings = _exportProvider.GetExportedValue<IRSettings>();
            var dateTime = DateTime.Now;
            settings.Connections = new[] {
                new ConnectionInfo("A", "http://127.0.0.1", null, true) { LastUsed = dateTime.AddHours(-4) },
                new ConnectionInfo("B", "http://127.0.0.2", null, true) { LastUsed = dateTime.AddHours(-3) },
                new ConnectionInfo("C", "http://127.0.0.3", null, true) { LastUsed = dateTime.AddHours(-2) },
                new ConnectionInfo("D", "http://127.0.0.4", null, true) { LastUsed = dateTime.AddHours(-1) }
            };

            var cm = _exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate().Connections;
            var connection = cm.RecentConnections.First(c => !c.IsRemote);
            await cm.ConnectAsync(connection).Should().BeCompletedAsync();

            cm.RecentConnections.Should().StartWith(connection);
            cm.ActiveConnection.Should().Be(connection);
        }

        [Test]
        public async Task RecentConnections_AfterSwitch_AddNewConnection() {
            var settings = _exportProvider.GetExportedValue<IRSettings>();
            var dateTime = DateTime.Now;
            settings.Connections = new[] {
                new ConnectionInfo("A", "http://127.0.0.1", null, true) { LastUsed = dateTime.AddHours(-4) },
                new ConnectionInfo("B", "http://127.0.0.2", null, true) { LastUsed = dateTime.AddHours(-3) },
                new ConnectionInfo("C", "http://127.0.0.3", null, true) { LastUsed = dateTime.AddHours(-2) }
            };

            var cm = _exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate().Connections;
            var connection = cm.RecentConnections.First(c => !c.IsRemote);
            await cm.ConnectAsync(connection).Should().BeCompletedAsync();
            cm.GetOrAddConnection("D", "http://127.0.0.4", null, false);

            cm.RecentConnections.Should().StartWith(connection);
            cm.ActiveConnection.Should().Be(connection);
        }

        public void Dispose() {
            _exportProvider.Dispose();
        }
    }
}