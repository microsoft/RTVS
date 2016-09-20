// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            Model?.CancelEdit();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e) {
            Model?.Save(GetConnection(e));
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e) {
            Model?.ConnectAsync(GetConnection(e)).DoNotWait();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e) {
            Model?.EditNew();
        }

        private void ButtonPath_Click(object sender, RoutedEventArgs e) {
            Model?.BrowseLocalPath(GetConnection(e));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e) {
            Model?.Edit(GetConnection(e));
            ScrollEditedIntoView(); ;
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e) {
            Model?.TryDelete(GetConnection(e));
        }

        private void ButtonTestConnection_Click(object sender, RoutedEventArgs e) {
            Model?.TestConnectionAsync(GetConnection(e)).DoNotWait();
        }

        private void ButtonCancelTestConnection_Click(object sender, RoutedEventArgs e) {
            Model?.CancelTestConnection(GetConnection(e));
        }

        private static IConnectionViewModel GetConnection(RoutedEventArgs e) => ((FrameworkElement)e.Source).DataContext as IConnectionViewModel;

        private void ScrollEditedIntoView() {
            var model = Model;
            if (model != null && !model.IsEditingNew && model.EditedConnection != null) {
                var list = model.EditedConnection.IsRemote ? RemoteList : LocalList;
                list.ScrollIntoView(model.EditedConnection);
                list.SelectedItems.Add(model.EditedConnection);
            }
        }

        private void Connection_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Model?.ConnectAsync(GetConnection(e)).DoNotWait();
            }
        }

        private void Connection_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            Model?.ConnectAsync(GetConnection(e)).DoNotWait();
            e.Handled = true;
        }
    }
}
