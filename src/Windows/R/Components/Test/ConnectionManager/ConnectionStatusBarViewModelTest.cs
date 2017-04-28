// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.ConnectionManager.Implementation.ViewModel;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;

namespace Microsoft.R.Components.Test.ConnectionManager {
    [ExcludeFromCodeCoverage]
    [Category.Connections]
    public sealed class ConnectionStatusBarViewModelTest {
        private readonly IConnectionManager _cm = Substitute.For<IConnectionManager>();
        private readonly ICoreShell _shell = TestCoreShell.CreateBasic();

        [Test]
        public void Construction() {
            var m = new ConnectionStatusBarViewModel(_cm, _shell.Services);
            m.SelectedConnection.Should().BeNullOrEmpty();
            m.IsActive.Should().BeFalse();
            m.IsConnected.Should().BeFalse();
            m.IsRunning.Should().BeFalse();
        }

        [Test]
        public void ConnectStates() {
            var m = new ConnectionStatusBarViewModel(_cm, _shell.Services) {
                IsConnected = true
            };
            m.IsRunning.Should().BeFalse();

            m.IsRunning = true;
            m.IsRunning = false;
            m.IsConnected.Should().BeTrue();
        }

        [Test(ThreadType.UI)]
        public async Task StateChanges() {
            var m = new ConnectionStatusBarViewModel(_cm, _shell.Services);

            _cm.IsConnected.Returns(true);
            _cm.ConnectionStateChanged += Raise.EventWith(_cm, EventArgs.Empty);

            await UIThreadHelper.Instance.DoEventsAsync(); // Event is dispatched to main thread
            m.IsConnected.Should().BeTrue();
            m.IsRunning.Should().BeFalse();

            _cm.IsRunning.Returns(true);
            _cm.ConnectionStateChanged += Raise.EventWith(_cm, EventArgs.Empty);

            await UIThreadHelper.Instance.DoEventsAsync(); // Event is dispatched to main thread
            m.IsConnected.Should().BeTrue();
            m.IsRunning.Should().BeTrue();

            _cm.IsRunning.Returns(false);
            _cm.ConnectionStateChanged += Raise.EventWith(_cm, EventArgs.Empty);

            await UIThreadHelper.Instance.DoEventsAsync(); // Event is dispatched to main thread
            m.IsConnected.Should().BeTrue();
            m.IsRunning.Should().BeFalse();

            _cm.IsConnected.Returns(false);
            _cm.ConnectionStateChanged += Raise.EventWith(_cm, EventArgs.Empty);

            await UIThreadHelper.Instance.DoEventsAsync(); // Event is dispatched to main thread
            m.IsConnected.Should().BeFalse();
            m.IsRunning.Should().BeFalse();
        }
    }
}
