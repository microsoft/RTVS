using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Common.Core;
using Microsoft.R.Components.ConnectionManager.ViewModel;

namespace Microsoft.R.Components.ConnectionManager.Implementation.View {
    /// <summary>
    /// Interaction logic for ConnectionManagerControl.xaml
    /// </summary>
    public partial class ConnectionManagerControl : UserControl {

        private IConnectionManagerViewModel Model => DataContext as IConnectionManagerViewModel;

        public ConnectionManagerControl() {
            InitializeComponent();
        }


        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var connection = e.AddedItems.OfType<IConnectionViewModel>().FirstOrDefault();
            if (connection != null) {
                Model.SelectConnection(connection);
                List.ScrollIntoView(connection);
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            Model?.CancelNew();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e) {
            Model?.SaveSelected();
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e) {
            Model?.ConnectAsync(GetConnection(e)).DoNotWait();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e) {
            Model?.AddNew();
        }

        private void ButtonPath_Click(object sender, RoutedEventArgs e) {
            Model?.BrowseLocalPath(GetConnection(e));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e) {
            Model?.Edit(GetConnection(e));
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e) {
            Model?.TryDelete(GetConnection(e));
        }

        private void ButtonTestConnection_Click(object sender, RoutedEventArgs e) {
            Model?.TestConnectionAsync(GetConnection(e)).DoNotWait();
        }

        private static IConnectionViewModel GetConnection(RoutedEventArgs e) => ((FrameworkElement)e.Source).DataContext as IConnectionViewModel;
    }
}
