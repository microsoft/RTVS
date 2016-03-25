// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.R.Components.PackageManager.Implementation.View {
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
