// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Plots.Implementation;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Test.Fakes.VisualComponentFactories {
    [ExcludeFromCodeCoverage]
    [Export(typeof (IRPlotHistoryVisualComponentContainerFactory))]
    internal sealed class TestRPlotHistoryVisualComponentContainerFactory : ContainerFactoryBase<IRPlotHistoryVisualComponent>, IRPlotHistoryVisualComponentContainerFactory {
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public TestRPlotHistoryVisualComponentContainerFactory(ICoreShell coreShell) {
            _coreShell = coreShell;
        }

        public PlotDeviceProperties DeviceProperties { get; set; } = new PlotDeviceProperties(360, 360, 96);

        public IVisualComponentContainer<IRPlotHistoryVisualComponent> GetOrCreate(IRPlotManager plotManager, int instanceId = 0) {
            return GetOrCreate(instanceId, container => new RPlotHistoryVisualComponent(plotManager, container, _coreShell.Services));
        }
    }
}
