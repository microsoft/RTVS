// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
