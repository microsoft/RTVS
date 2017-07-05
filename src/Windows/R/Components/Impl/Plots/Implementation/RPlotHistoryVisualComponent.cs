// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.Plots.Implementation.View;
using Microsoft.R.Components.Plots.Implementation.ViewModel;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Components.View;

namespace Microsoft.R.Components.Plots.Implementation {
    public class RPlotHistoryVisualComponent : IRPlotHistoryVisualComponent {
        private readonly DisposableBag _disposableBag;
        private readonly IRPlotHistoryViewModel _viewModel;

        public RPlotHistoryVisualComponent(IRPlotManager plotManager, IVisualComponentContainer<IRPlotHistoryVisualComponent> container, IServiceContainer services) {
            Check.ArgumentNull(nameof(plotManager), plotManager);
            Check.ArgumentNull(nameof(container), container);
            Check.ArgumentNull(nameof(services), services);

            var control = new RPlotHistoryControl();
            _viewModel = new RPlotHistoryViewModel(control, plotManager, services.MainThread());
            control.DataContext = _viewModel;

            _disposableBag = DisposableBag.Create<RPlotDeviceVisualComponent>()
                .Add(() => control.ContextMenuRequested -= Control_ContextMenuRequested);

            control.ContextMenuRequested += Control_ContextMenuRequested;

            Control = control;
            Container = container;
        }

        public void Dispose() {
            _disposableBag.TryDispose();
        }

        public FrameworkElement Control { get; }

        public IVisualComponentContainer<IVisualComponent> Container { get; }

        public IEnumerable<IRPlot> SelectedPlots {
            get => _viewModel.SelectedPlots.Select(m => m.Plot);
            set => _viewModel.SelectEntry(value?.FirstOrDefault());
        }

        public bool CanDecreaseThumbnailSize => _viewModel.ThumbnailSize > RPlotHistoryViewModel.MinThumbnailSize;
        public bool CanIncreaseThumbnailSize => _viewModel.ThumbnailSize < RPlotHistoryViewModel.MaxThumbnailSize;

        public bool AutoHide {
            get => _viewModel.AutoHide;
            set => _viewModel.AutoHide = value;
        }

        private void Control_ContextMenuRequested(object sender, PointEventArgs e) 
            => Container.ShowContextMenu(RPlotCommandIds.PlotHistoryContextMenu, e.Point);

        public void DecreaseThumbnailSize() => _viewModel.DecreaseThumbnailSize();
        public void IncreaseThumbnailSize() => _viewModel.IncreaseThumbnailSize();
    }
}
