// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.ConnectionManager.ViewModel;
using Microsoft.R.Wpf;
using Microsoft.R.Wpf.Themes;

namespace Microsoft.R.Components.ConnectionManager.Implementation.View {
    /// <summary>
    /// Interaction logic for ConnectionManagerControl.xaml
    /// </summary>
    public partial class ConnectionManagerControl : UserControl {
        private readonly IThemeUtilities _theme;
        private IConnectionManagerViewModel Model => DataContext as IConnectionManagerViewModel;

        public ConnectionManagerControl(IServiceContainer services) {
            InitializeComponent();

            _theme = services.GetService<IThemeUtilities>();
            var ui = services.UI();
            ui.UIThemeChanged += OnUIThemeChanged;
            SetImageBackground();
        }

        private void OnUIThemeChanged(object sender, System.EventArgs e) => SetImageBackground();

        private void SetImageBackground() {
            _theme.SetImageBackgroundColor(this, Brushes.ToolWindowBackgroundColorKey);
            _theme.SetThemeScrollBars(this);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) => Model?.CancelEdit();
        private void ButtonSave_Click(object sender, RoutedEventArgs e) => Model?.Save(GetConnection(e));
        private void ButtonConnect_Click(object sender, RoutedEventArgs e) => HandleConnect(e, true);
        private void ButtonAdd_Click(object sender, RoutedEventArgs e) => Model?.TryEditNew();
        private void ButtonPath_Click(object sender, RoutedEventArgs e) => Model?.BrowseLocalPath(GetConnection(e));

        private void ButtonEdit_Click(object sender, RoutedEventArgs e) {
            if (Model?.TryEdit(GetConnection(e)) == true) {
                ScrollEditedIntoView();
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e) => Model?.TryDelete(GetConnection(e));
        private void ButtonTestConnection_Click(object sender, RoutedEventArgs e) => Model?.TestConnectionAsync(GetConnection(e)).DoNotWait();
        private void ButtonCancelTestConnection_Click(object sender, RoutedEventArgs e) => Model?.CancelTestConnection();

        private static IConnectionViewModel GetConnection(RoutedEventArgs e) => ((FrameworkElement)e.Source).DataContext as IConnectionViewModel;

        private void ScrollEditedIntoView() {
            var model = Model;
            if (model != null && !model.IsEditingNew && model.EditedConnection != null) {
                var list = model.EditedConnection.IsRemote ? RemoteList : LocalList;
                list.ScrollIntoView(model.EditedConnection);
                list.SelectedItems.Add(model.EditedConnection);
            }
        }

        private void Connection_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete && !(e.OriginalSource is TextBox)) {
                Model?.TryDelete(GetConnection(e));
            }
        }

        private void Connection_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            HandleConnect(e, false);
            e.Handled = true;
        }

        private void EditConnection_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                Model?.CancelEdit();
            }
        }

        private void PathTextBox_LostFocus(object sender, RoutedEventArgs e) {
            ((sender as TextBox)?.DataContext as IConnectionViewModel)?.UpdatePath();
        }

        private void TextBoxName_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != e.OldValue && (bool)e.NewValue) {
                (sender as TextBox)?.Focus();
            }
        }

        private void HandleConnect(RoutedEventArgs e, bool connectToEdited) => Model?.Connect(GetConnection(e), connectToEdited);
    }
}
