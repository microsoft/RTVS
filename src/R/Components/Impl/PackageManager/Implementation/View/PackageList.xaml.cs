// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            Model?.Update(package);
        }

        private static IRPackageViewModel GetPackage(RoutedEventArgs e) {
            return ((IRPackageViewModel)((FrameworkElement)e.Source).DataContext);
        }

        private void List_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                HandleDefaultAction();
            }
        }

        private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            HandleDefaultAction();
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Model.SelectPackage(e.AddedItems.OfType<IRPackageViewModel>().FirstOrDefault());
        }

        private void ButtonUninstall_Click(object sender, RoutedEventArgs e) {
            var package = GetPackage(e);
            Model?.Uninstall(package);
        }

        private void ButtonInstall_Click(object sender, RoutedEventArgs e) {
            var package = GetPackage(e);
            Model?.Install(package);
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e) {
            var package = GetPackage(e);
            Model?.Load(package);
        }

        private void ButtonUnload_Click(object sender, RoutedEventArgs e) {
            var package = GetPackage(e);
            Model?.Unload(package);
        }

        private void HandleDefaultAction() {
            if (Model == null || Model.SelectedPackage == null) {
                return;
            }
            // Available => Installed => Loaded
            var package = Model.SelectedPackage;
            if (!package.IsInstalled) {
                Model.Install(package);
            } else if (!package.IsLoaded) {
                Model.Load(package);
            }
        }
    }
}
