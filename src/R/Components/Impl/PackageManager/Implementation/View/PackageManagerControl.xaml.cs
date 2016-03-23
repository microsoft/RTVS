using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.R.Components.PackageManager.ViewModel;

namespace Microsoft.R.Components.PackageManager.Implementation.View {
    /// <summary>
    /// Interaction logic for PackageManagerControl.xaml
    /// </summary>
    public partial class PackageManagerControl : UserControl {
        private IRPackageManagerViewModel Model => DataContext as IRPackageManagerViewModel;

        public PackageManagerControl() {
            InitializeComponent();
        }

        private void CheckBoxSuppressLegalDisclaimer_Checked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void ListPackages_Loaded(object sender, RoutedEventArgs e) {

        }
    }
}
