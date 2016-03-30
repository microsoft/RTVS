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
        private ScrollViewer _scrollViewer;

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
            throw new NotImplementedException();
        }

        private void List_PreviewKeyUp(object sender, KeyEventArgs e) {
            throw new NotImplementedException();
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Model.SelectPackage(e.AddedItems.OfType<IRPackageViewModel>().FirstOrDefault());
        }

        private void ButtonUninstall_Click(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void ButtonInstall_Click(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }
    }
}
