// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Information;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Protocol;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;

namespace Microsoft.R.Components.Test.Information {
    [ExcludeFromCodeCoverage]
    [Category.Information]
    public sealed class HostLoadIndicatorViewModelTest {
        private readonly IRInteractiveWorkflow _workflow = Substitute.For<IRInteractiveWorkflow>();
        private readonly IRSessionProvider _sessionProvider = Substitute.For<IRSessionProvider>();
        private readonly IBrokerClient _brokerClient = Substitute.For<IBrokerClient>();
        private readonly ICoreShell _coreShell = Substitute.For<ICoreShell>();

        private readonly HostLoad _hostLoad = new HostLoad() {
            CpuLoad = 30,
            MemoryLoad = 40,
            NetworkLoad = 50
        };

        public HostLoadIndicatorViewModelTest() {
            _sessionProvider.Broker.Returns(_brokerClient);
            _workflow.RSessions.Returns(_sessionProvider);
            _workflow.Shell.Returns(_coreShell);

            _coreShell.When(x => x.DispatchOnUIThread(Arg.Any<Action>())).Do(c => ((Action)c.Args()[0])());
            _brokerClient.GetHostInformationAsync<HostLoad>().Returns(Task.FromResult(_hostLoad));
        }

        [Test]
        public async Task Update01() {
            var m = new HostLoadIndicatorViewModel(_workflow, 0);
            await m.UpdateModelAsync();

            m.CpuLoad.Should().Be(30);
            m.MemoryLoad.Should().Be(40);
            m.NetworkLoad.Should().Be(50);
            m.Tooltip.Should().Contain("30").And.Contain("40").And.Contain("50");
         }

        [Test]
        public async Task Update02() {
            var m = new HostLoadIndicatorViewModel(_workflow, 10);
            await m.UpdateModelAsync();
            await Task.Delay(100);

            m.CpuLoad.Should().Be(30);
            m.MemoryLoad.Should().Be(40);
            m.NetworkLoad.Should().Be(50);
            m.Tooltip.Should().Contain("30").And.Contain("40").And.Contain("50");
        }
    }
}
