// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using Microsoft.R.Components.PackageManager.ViewModel;

namespace Microsoft.R.Components.PackageManager.Implementation {
    internal class RPackageManagerViewModel : IRPackageManagerViewModel {
        public ObservableCollection<object> Items { get; }
        public IRPackageViewModel SelectedPackage { get; }

        private SelectedTab _selectedTab;

        public void SwitchToAvailablePackages() {
            if (_selectedTab == SelectedTab.AvailablePackages) {
                return;
            }

            _selectedTab = SelectedTab.AvailablePackages;
            ReloadItems();
        }

        public void SwitchToInstalledPackages() {
            if (_selectedTab == SelectedTab.InstalledPackages) {
                return;
            }

            _selectedTab = SelectedTab.InstalledPackages;
            ReloadItems();
        }

        public void SwitchToLoadedPackages() {
            if (_selectedTab == SelectedTab.LoadedPackages) {
                return;
            }

            _selectedTab = SelectedTab.LoadedPackages;
            ReloadItems();
        }

        public void ReloadItems() {
            switch (_selectedTab) {
                case SelectedTab.AvailablePackages:
                    LoadAvailablePackages();
                    break;
                case SelectedTab.InstalledPackages:
                    LoadInstalledPackages();
                    break;
                case SelectedTab.LoadedPackages:
                    LoadLoadedPackages();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void LoadAvailablePackages() {
        }

        private void LoadInstalledPackages() {
        }

        private void LoadLoadedPackages() {
        }

        private enum SelectedTab {
            AvailablePackages,
            InstalledPackages,
            LoadedPackages,
        }
    }
}
