using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.ConnectionManager.Implementation;
using Microsoft.R.Components.View;

namespace Microsoft.R.Components.Test.Fakes.VisualComponentFactories {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IConnectionManagerVisualProvider))]
    internal sealed class TestConnectionManagerVisualProvider : ContainerFactoryBase<IConnectionManagerVisual>, IConnectionManagerVisualProvider {
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public TestConnectionManagerVisualProvider(ICoreShell shell) {
            _shell = shell;
        }

        public IConnectionManagerVisual GetOrCreate(IConnectionManager connectionManager, int instanceId = 0)
            => GetOrCreate(instanceId, container => new ConnectionManagerVisual(connectionManager, container, _shell.Services)).Component;
    }
}