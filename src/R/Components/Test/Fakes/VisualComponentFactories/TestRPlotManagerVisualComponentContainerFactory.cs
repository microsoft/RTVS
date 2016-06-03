using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.PackageManager.Implementation;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Plots.Implementation;
using Microsoft.R.Components.Search;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Test.Fakes.VisualComponentFactories {
    [Export(typeof (IRPlotManagerVisualComponentContainerFactory))]
    internal sealed class TestRPlotManagerVisualComponentContainerFactory : ContainerFactoryBase<IRPlotManagerVisualComponent>, IRPlotManagerVisualComponentContainerFactory {
        private readonly IRSettings _settings;
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public TestRPlotManagerVisualComponentContainerFactory(IRSettings settings, ICoreShell coreShell) {
            _settings = settings;
            _coreShell = coreShell;
        }

        public IVisualComponentContainer<IRPlotManagerVisualComponent> GetOrCreate(IRPlotManager plotManager, IRSession session, int instanceId = 0) {
            return GetOrCreate(instanceId, container => new RPlotManagerVisualComponent(plotManager, container, session, _settings, _coreShell));
        }
    }
}