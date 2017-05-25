using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.ConnectionManager.Implementation;
using Microsoft.R.Components.View;

namespace Microsoft.R.Components.Test.Fakes.VisualComponentFactories {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IConnectionManagerVisualComponentContainerFactory))]
    internal sealed class TestConnectionManagerVisualComponentContainerFactory : ContainerFactoryBase<IConnectionManagerVisualComponent>, IConnectionManagerVisualComponentContainerFactory {
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public TestConnectionManagerVisualComponentContainerFactory(ICoreShell shell) {
            _shell = shell;
        }

        public IVisualComponentContainer<IConnectionManagerVisualComponent> GetOrCreate(IConnectionManager connectionManager, int instanceId = 0)
            => GetOrCreate(instanceId, container => new ConnectionManagerVisualComponent(connectionManager, container, _shell.Services));
    }
}