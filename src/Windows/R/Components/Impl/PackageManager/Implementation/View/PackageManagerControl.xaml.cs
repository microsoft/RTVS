// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.PackageManager.ViewModel;

namespace Microsoft.R.Components.PackageManager.Implementation.View {
    /// <summary>
    /// Interaction logic for PackageManagerControl.xaml
    /// </summary>
    public partial class PackageManagerControl : UserControl {
        public IRPackageManagerViewModel Model => DataContext as IRPackageManagerViewModel;

        public PackageManagerControl(IServiceContainer services) {
            InitializeComponent();
            ListPackages.Initialize(services);
        }

        private void CheckBoxSuppressLegalDisclaimer_Checked(object sender, RoutedEventArgs e) {
            if (Model != null) {
                Model.ShowPackageManagerDisclaimer = false;
            }
        }

        private void TabLoaded_Checked(object sender, RoutedEventArgs e) {
            Model?.SwitchToLoadedPackagesAsync().DoNotWait();
        }

        private void TabInstalled_Checked(object sender, RoutedEventArgs e) {
            Model?.SwitchToInstalledPackagesAsync().DoNotWait();
        }

        private void TabAvailable_Checked(object sender, RoutedEventArgs e) {
            Model?.SwitchToAvailablePackagesAsync().DoNotWait();
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void ListPackages_Loaded(object sender, RoutedEventArgs e) {
            TabInstalled.IsChecked = true;
        }
    }
}
