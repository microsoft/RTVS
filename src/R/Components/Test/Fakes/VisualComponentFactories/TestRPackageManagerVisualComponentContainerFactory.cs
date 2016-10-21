using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.PackageManager.Implementation;
using Microsoft.R.Components.Search;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Test.Fakes.VisualComponentFactories {
    [ExcludeFromCodeCoverage]
    [Export(typeof (IRPackageManagerVisualComponentContainerFactory))]
    internal sealed class TestRPackageManagerVisualComponentContainerFactory : ContainerFactoryBase<IRPackageManagerVisualComponent>, IRPackageManagerVisualComponentContainerFactory {
        private readonly IRSettings _settings;
        private readonly ICoreShell _coreShell;
        private readonly ISearchControlProvider _searchControlProvider;

        [ImportingConstructor]
        public TestRPackageManagerVisualComponentContainerFactory(ISearchControlProvider searchControlProvider, IRSettings settings, ICoreShell coreShell) {
            _searchControlProvider = searchControlProvider;
            _settings = settings;
            _coreShell = coreShell;
        }

        public IVisualComponentContainer<IRPackageManagerVisualComponent> GetOrCreate(IRPackageManager packageManager, IRSession session, int instanceId = 0) {
            return GetOrCreate(instanceId, container => new RPackageManagerVisualComponent(packageManager, container, session, _searchControlProvider, _settings, _coreShell));
        }
    }
}