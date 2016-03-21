using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Microsoft.R.Components.PackageManager.Implementation.Xaml {
    /// <summary>
    /// Interaction logic for PackageManagerTopPanel.xaml
    /// </summary>
    public partial class PackageManagerTopPanel : UserControl {
        public PackageManagerTopPanel() {
            InitializeComponent();
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }
        
        private void TabLoaded_Checked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void TabInstalled_Checked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void TabAvailable_Checked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }
    }
}
