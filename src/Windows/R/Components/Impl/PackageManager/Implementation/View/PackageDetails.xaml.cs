// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Common.Core;
using Microsoft.R.Components.PackageManager.ViewModel;

namespace Microsoft.R.Components.PackageManager.Implementation.View {
    /// <summary>
    /// Interaction logic for PackageDetails.xaml
    /// </summary>
    public partial class PackageDetails : UserControl {
        private IRPackageViewModel Model => DataContext as IRPackageViewModel;

        public PackageDetails() {
            InitializeComponent();
        }

        private void ButtonInstall_Click(object sender, RoutedEventArgs e) {
            Model?.InstallAsync().DoNotWait();
        }

        private void ButtonUninstall_Click(object sender, RoutedEventArgs e) {
            Model?.UninstallAsync().DoNotWait();
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e) {
            Model?.UpdateAsync().DoNotWait();
        }

        private void RepositoryUri_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(e.Uri.AbsoluteUri);
        }

        private void LibraryPath_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            var path = e.Uri.ToString().Replace('/', '\\');
            if (Directory.Exists(path)) {
                try {
                    Process.Start(path);
                } catch (Win32Exception) { }
            }
        }

        private void Urls_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(e.Uri.AbsoluteUri);
        }
    }
}
