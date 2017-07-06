// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InfoBar;
using Microsoft.R.Components.PackageManager.Implementation.ViewModel;
using Microsoft.R.Components.PackageManager.ViewModel;
using Microsoft.R.Components.Search;
using Microsoft.R.Components.View;
using PackageManagerControl = Microsoft.R.Components.PackageManager.Implementation.View.PackageManagerControl;

namespace Microsoft.R.Components.PackageManager.Implementation {
    public class RPackageManagerVisualComponent : IRPackageManagerVisualComponent {
        private readonly IRPackageManagerViewModel _viewModel;
        private readonly Guid SearchCategory = new Guid("B3A0CF4D-FC8A-47AB-8604-5D2EEF73872F");
        private readonly ISearchControl _searchControl;

        public RPackageManagerVisualComponent(IRPackageManager packageManager, IVisualComponentContainer<IRPackageManagerVisualComponent> container, ISearchControlProvider searchControlProvider, IServiceContainer services) {
            Container = container;
            var control = new PackageManagerControl(services);
            Control = control;

            var infoBarProvider = services.GetService<IInfoBarProvider>();
            var infoBar = infoBarProvider.Create(control.InfoBarControlHost);
            _viewModel = new RPackageManagerViewModel(packageManager, infoBar, services);
            
            var searchControlSettings = new SearchControlSettings {
                SearchCategory = SearchCategory,
                MinWidth = (uint)control.SearchControlHost.MinWidth,
                MaxWidth = uint.MaxValue
            };
            _searchControl = searchControlProvider.Create(control.SearchControlHost, _viewModel, searchControlSettings);

            control.DataContext = _viewModel;
        }

        public void Dispose() {
            _searchControl.Dispose();
            _viewModel.Dispose();
        }

        public FrameworkElement Control { get; }
        public IVisualComponentContainer<IVisualComponent> Container { get; }
    }
}
