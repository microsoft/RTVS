// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Common.Core;
using Microsoft.R.Components.PackageManager.ViewModel;

namespace Microsoft.R.Components.PackageManager.Implementation.View {
    public partial class PackageList : UserControl {
        private IRPackageManagerViewModel Model => DataContext as IRPackageManagerViewModel;

        // Indicates wether check boxes are enabled on packages
        private bool _checkBoxesEnabled;

        public bool CheckBoxesEnabled {
            get { return _checkBoxesEnabled; }
            set {
                _checkBoxesEnabled = value;

                if (!_checkBoxesEnabled) {
                    // the current tab is not "updates", so the container
                    // should become invisible.
                    UpdateButtonContainer.Visibility = Visibility.Collapsed;
                }
            }
        }

        public PackageList() {
            InitializeComponent();
            CheckBoxesEnabled = false;
        }

        private void CheckBoxSelectAllPackages_Checked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void CheckBoxSelectAllPackages_Unchecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e) {
            var package = GetPackage(e);
            Model?.UpdateAsync(package).DoNotWait();
        }

        private static IRPackageViewModel GetPackage(RoutedEventArgs e) {
            return ((IRPackageViewModel)((FrameworkElement)e.Source).DataContext);
        }

        private void List_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Model?.DefaultActionAsync().DoNotWait();
            }
        }

        private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            Model?.DefaultActionAsync().DoNotWait();
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var package = e.AddedItems.OfType<IRPackageViewModel>().FirstOrDefault();
            if (package != null) {
                Model.SelectPackage(package);
                List.ScrollIntoView(package);
            }
        }

        private void ButtonUninstall_Click(object sender, RoutedEventArgs e) {
            var package = GetPackage(e);
            Model?.UninstallAsync(package).DoNotWait();
        }

        private void ButtonInstall_Click(object sender, RoutedEventArgs e) {
            var package = GetPackage(e);
            Model?.InstallAsync(package).DoNotWait();
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e) {
            var package = GetPackage(e);
            Model?.LoadAsync(package).DoNotWait();
        }

        private void ButtonUnload_Click(object sender, RoutedEventArgs e) {
            var package = GetPackage(e);
            Model?.UnloadAsync(package).DoNotWait();
        }
    }
}
