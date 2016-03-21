using System.Windows;
using System.Windows.Controls;

namespace Microsoft.R.Components.PackageManager.Implementation.Xaml {
    internal partial class PackageManagerResources {
        public PackageManagerResources() {
            InitializeComponent();
        }

        private void PackageIconImage_ImageFailed(object sender, ExceptionRoutedEventArgs e) {
            var image = sender as Image;
            if (image != null) {
                image.Source = Images.DefaultPackageIcon;
            }
        }
    }
}
