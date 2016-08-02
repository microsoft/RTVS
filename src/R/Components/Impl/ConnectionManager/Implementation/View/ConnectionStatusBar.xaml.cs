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
using Microsoft.R.Components.ConnectionManager.ViewModel;

namespace Microsoft.R.Components.ConnectionManager.Implementation.View {
    /// <summary>
    /// Interaction logic for ConnectionStatusBar.xaml
    /// </summary>
    public partial class ConnectionStatusBar : UserControl {
        public IConnectionStatusBarViewModel Model => DataContext as IConnectionStatusBarViewModel;

        public ConnectionStatusBar() {
            InitializeComponent();
        }

        protected override void OnMouseUp(MouseButtonEventArgs e) {
            base.OnMouseUp(e);
            Model?.ShowContextMenu(PointToScreen(new Point(ActualWidth, 0)));
            e.Handled = true;
        }
    }
}
