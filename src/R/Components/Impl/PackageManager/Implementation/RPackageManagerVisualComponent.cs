// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.PackageManager.Implementation.ViewModel;
using Microsoft.R.Components.PackageManager.ViewModel;
using Microsoft.R.Components.Search;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.View;
using Microsoft.R.Wpf;
using PackageManagerControl = Microsoft.R.Components.PackageManager.Implementation.View.PackageManagerControl;

namespace Microsoft.R.Components.PackageManager.Implementation {
    public class RPackageManagerVisualComponent : IRPackageManagerVisualComponent {
        private readonly IRPackageManagerViewModel _viewModel;
        private readonly Guid SearchCategory = new Guid("B3A0CF4D-FC8A-47AB-8604-5D2EEF73872F");
        private readonly ISearchControl _searchControl;

        public RPackageManagerVisualComponent(IRPackageManager packageManager, IVisualComponentContainer<IRPackageManagerVisualComponent> container, ISearchControlProvider searchControlProvider, IRSettings settings, ICoreShell coreShell) {
            _viewModel = new RPackageManagerViewModel(packageManager, settings, coreShell);
            Container = container;
            Controller = null;
            var control = new PackageManagerControl {
                DataContext = _viewModel,
            };
            Control = control;
            var searchControlSettings = new SearchControlSettings {
                SearchCategory = SearchCategory,
                MinWidth = (uint)control.SearchControlHost.MinWidth,
                MaxWidth = uint.MaxValue
            };
            _searchControl = searchControlProvider.Create(control.SearchControlHost, _viewModel, searchControlSettings);
        }

        public void Dispose() {
            _searchControl.Dispose();
            _viewModel.Dispose();
        }

        public ICommandTarget Controller { get; }
        public FrameworkElement Control { get; }
        public IVisualComponentContainer<IVisualComponent> Container { get; }
    }
}
