// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.ConnectionManager.Implementation.ViewModel;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;

namespace Microsoft.R.Components.Test.ConnectionManager {
    [ExcludeFromCodeCoverage]
    [Category.Connections]
    public sealed class ConnectionStatusBarViewModelTest {
        private readonly IConnectionManager _cm;
        private readonly ICoreShell _shell;

        public ConnectionStatusBarViewModelTest() {
            _cm = Substitute.For<IConnectionManager>();
            _shell = Substitute.For<ICoreShell>();
            _shell.When(x => x.DispatchOnUIThread(Arg.Any<Action>())).Do(c => ((Action)c.Args()[0])());
         }

        [Test]
        public void Construction() {
            var m = new ConnectionStatusBarViewModel(_cm, _shell);
            m.SelectedConnection.Should().BeNullOrEmpty();
            m.IsActive.Should().BeFalse();
            m.IsConnected.Should().BeFalse();
            m.IsRunning.Should().BeFalse();
        }

        [Test]
        public void ConnectStates() {
            var m = new ConnectionStatusBarViewModel(_cm, _shell);
            m.IsConnected = true;
            m.IsRunning.Should().BeFalse();

            m.IsRunning = true;
            m.IsConnected = false;
            m.IsRunning.Should().BeFalse();

            m.IsRunning = false;
            m.IsConnected = true;
            m.IsRunning.Should().BeFalse();
        }

        [Test]
        public void StateChanges() {
            var m = new ConnectionStatusBarViewModel(_cm, _shell);

            _cm.IsConnected.Returns(true);
            _cm.ConnectionStateChanged += Raise.EventWith(_cm, EventArgs.Empty);

            m.IsConnected.Should().BeTrue();
            m.IsRunning.Should().BeFalse();

            _cm.IsRunning.Returns(true);
            _cm.ConnectionStateChanged += Raise.EventWith(_cm, EventArgs.Empty);

            m.IsConnected.Should().BeTrue();
            m.IsRunning.Should().BeTrue();

            _cm.IsConnected.Returns(false);
            _cm.ConnectionStateChanged += Raise.EventWith(_cm, EventArgs.Empty);

            m.IsConnected.Should().BeFalse();
            m.IsRunning.Should().BeFalse();
        }
    }
}
