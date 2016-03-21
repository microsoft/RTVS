using System.Windows;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.PackageManager.Implementation.Xaml;
using Microsoft.R.Components.View;

namespace Microsoft.R.Components.PackageManager.Implementation {
    public class RPackageManagerVisualComponent : IRPackageManagerVisualComponent {
        public RPackageManagerVisualComponent(IVisualComponentContainer<IRPackageManagerVisualComponent> container) {
            Container = container;
            Controller = null;
            Control = new PackageManagerControl();
        }

        public void Dispose() {
            throw new System.NotImplementedException();
        }

        public ICommandTarget Controller { get; }
        public FrameworkElement Control { get; }
        public IVisualComponentContainer<IVisualComponent> Container { get; }
    }
}
