// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Microsoft.R.Components.PackageManager.Implementation.View {
    /// <summary>
    /// Interaction logic for PackageDetails.xaml
    /// </summary>
    public partial class PackageDetails : UserControl {
        public PackageDetails() {
            InitializeComponent();
        }

        private void HyperlinkUrl_Click(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void ButtonInstall_Click(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }
        
        private void ButtonUninstall_Click(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void RepositoryUri_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(e.Uri.AbsoluteUri);
        }

        private void LibraryPath_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(e.Uri.ToString().Replace('/', '\\'));
        }
    }
}
