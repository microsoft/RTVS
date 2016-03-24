// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.PackageManager.ViewModel;
using Microsoft.R.Components.View;
using Microsoft.R.Wpf;
using PackageManagerControl = Microsoft.R.Components.PackageManager.Implementation.View.PackageManagerControl;

namespace Microsoft.R.Components.PackageManager.Implementation {
    public class RPackageManagerVisualComponent : IRPackageManagerVisualComponent {
        private readonly IRPackageManagerViewModel _viewModel;
        private readonly IRPackageManager _packageManager;

        public RPackageManagerVisualComponent(IRPackageManager packageManager, IVisualComponentContainer<IRPackageManagerVisualComponent> container) {
            _packageManager = packageManager;
            _viewModel = new RPackageManagerViewModel();
            Container = container;
            Controller = null;
            Control = new PackageManagerControl {
                DataContext = _viewModel
            };
        }

        public void Dispose() {
        }

        public ICommandTarget Controller { get; }
        public FrameworkElement Control { get; }
        public IVisualComponentContainer<IVisualComponent> Container { get; }
    }
}
