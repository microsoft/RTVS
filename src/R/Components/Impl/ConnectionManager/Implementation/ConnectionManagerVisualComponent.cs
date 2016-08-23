// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ConnectionManager.Implementation.View;
using Microsoft.R.Components.ConnectionManager.Implementation.ViewModel;
using Microsoft.R.Components.ConnectionManager.ViewModel;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Search;
using Microsoft.R.Components.View;

namespace Microsoft.R.Components.ConnectionManager.Implementation {
    public class ConnectionManagerVisualComponent : IConnectionManagerVisualComponent {
        private readonly IConnectionManagerViewModel _viewModel;
        private readonly Guid SearchCategory = new Guid("391EE435-D4A2-4449-B0B3-15F89D62741C");
        private readonly ISearchControl _searchControl;

        public ConnectionManagerVisualComponent(IConnectionManager connectionManager, IVisualComponentContainer<IConnectionManagerVisualComponent> container, ISearchControlProvider searchControlProvider, ICoreShell coreShell) {
            _viewModel = new ConnectionManagerViewModel(connectionManager, coreShell);
            Container = container;
            Controller = null;
            var control = new ConnectionManagerControl {
                DataContext = _viewModel
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
