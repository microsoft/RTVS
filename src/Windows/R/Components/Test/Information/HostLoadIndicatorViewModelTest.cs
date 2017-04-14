// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Information;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Protocol;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;

namespace Microsoft.R.Components.Test.Information {
    [ExcludeFromCodeCoverage]
    [Category.Information]
    public sealed class HostLoadIndicatorViewModelTest {
        private readonly IRSessionProvider _sessionProvider = Substitute.For<IRSessionProvider>();
        private readonly ICoreShell _coreShell;

        private readonly HostLoad _hostLoad = new HostLoad() {
            CpuLoad = 30,
            MemoryLoad = 40,
            NetworkLoad = 50
        };

        public HostLoadIndicatorViewModelTest(RComponentsShellProviderFixture shellProvider) {
            _coreShell = shellProvider.CoreShell;
        }
        [Test(ThreadType.UI)]
        public async Task Update() {
            var viewModel = new HostLoadIndicatorViewModel(_sessionProvider, _coreShell.MainThread());
            var eventArgs = new HostLoadChangedEventArgs(_hostLoad);
            _sessionProvider.HostLoadChanged += Raise.Event<EventHandler<HostLoadChangedEventArgs>>(_sessionProvider, eventArgs);

            await UIThreadHelper.Instance.DoEventsAsync(); // Event is dispatched to main thread

            viewModel.CpuLoad.Should().Be(30);
            viewModel.MemoryLoad.Should().Be(40);
            viewModel.NetworkLoad.Should().Be(50);
            viewModel.Tooltip.Should().Contain("30").And.Contain("40").And.Contain("50");
         }
    }
}
