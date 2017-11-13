// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InfoBar;
using Microsoft.R.Components.PackageManager.Implementation.ViewModel;
using Microsoft.R.Components.PackageManager.ViewModel;
using Microsoft.R.Components.Search;

namespace Microsoft.R.Components.PackageManager.Implementation.View {
    /// <summary>
    /// Interaction logic for PackageManagerControl.xaml
    /// </summary>
    public partial class PackageManagerControl : IDisposable {
        public static readonly Guid SearchCategory = new Guid("B3A0CF4D-FC8A-47AB-8604-5D2EEF73872F");
        private IRPackageManagerViewModel ViewModel { get; }
        private readonly DisposableBag _disposable = DisposableBag.Create<PackageManagerControl>();

        public PackageManagerControl(IServiceContainer services) {
            InitializeComponent();
            ListPackages.Initialize(services);

            var infoBar = services.GetService<IInfoBarProvider>().Create(InfoBarControlHost);
            ViewModel = new RPackageManagerViewModel(services, infoBar);

            var searchControlProvider = services.GetService<ISearchControlProvider>();
            var searchControlSettings = new SearchControlSettings {
                SearchCategory = SearchCategory,
                MinWidth = (uint)SearchControlHost.MinWidth,
                MaxWidth = uint.MaxValue
            };

            _disposable
                .Add(searchControlProvider.Create(SearchControlHost, ViewModel, searchControlSettings))
                .Add(ViewModel);

            DataContext = ViewModel;
        }

        public void Dispose() => _disposable.TryDispose();

        private void CheckBoxSuppressLegalDisclaimer_Checked(object sender, RoutedEventArgs e) {
            if (ViewModel != null) {
                ViewModel.ShowPackageManagerDisclaimer = false;
            }
        }

        private void TabLoaded_Checked(object sender, RoutedEventArgs e) {
            ViewModel?.SwitchToLoadedPackagesAsync().DoNotWait();
        }

        private void TabInstalled_Checked(object sender, RoutedEventArgs e) {
            ViewModel?.SwitchToInstalledPackagesAsync().DoNotWait();
        }

        private void TabAvailable_Checked(object sender, RoutedEventArgs e) {
            ViewModel?.SwitchToAvailablePackagesAsync().DoNotWait();
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void ListPackages_Loaded(object sender, RoutedEventArgs e) {
            TabInstalled.IsChecked = true;
        }
    }
}
