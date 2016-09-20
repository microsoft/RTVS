// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Plots.Implementation.View;
using Microsoft.R.Components.Plots.Implementation.ViewModel;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Components.View;

namespace Microsoft.R.Components.Plots.Implementation {
    public class RPlotHistoryVisualComponent : IRPlotHistoryVisualComponent {
        private readonly DisposableBag _disposableBag;
        private readonly ICoreShell _shell;
        private readonly IRPlotManager _plotManager;
        private readonly IRPlotHistoryViewModel _viewModel;

        public RPlotHistoryVisualComponent(IRPlotManager plotManager, ICommandTarget controller, IVisualComponentContainer<IRPlotHistoryVisualComponent> container, ICoreShell coreShell) {
            if (plotManager == null) {
                throw new ArgumentNullException(nameof(plotManager));
            }

            if (container == null) {
                throw new ArgumentNullException(nameof(container));
            }

            if (coreShell == null) {
                throw new ArgumentNullException(nameof(coreShell));
            }

            _plotManager = plotManager;
            _viewModel = new RPlotHistoryViewModel(plotManager, coreShell);
            _shell = coreShell;

            var control = new RPlotHistoryControl {
                DataContext = _viewModel
            };

            _disposableBag = DisposableBag.Create<RPlotDeviceVisualComponent>()
                .Add(() => control.ContextMenuRequested -= Control_ContextMenuRequested);

            control.ContextMenuRequested += Control_ContextMenuRequested;

            Control = control;
            Controller = controller;
            Container = container;
        }

        public void Dispose() {
            _disposableBag.TryMarkDisposed();
        }

        public ICommandTarget Controller { get; }

        public FrameworkElement Control { get; }

        public IVisualComponentContainer<IVisualComponent> Container { get; }

        public IRPlot SelectedPlot {
            get {
                return _viewModel.SelectedPlot?.Plot;
            }
            set {
                _viewModel.SelectEntry(value);
            }
        }

        public bool CanDecreaseThumbnailSize {
            get {
                return _viewModel.ThumbnailSize > RPlotHistoryViewModel.MinThumbnailSize;
            }
        }

        public bool CanIncreaseThumbnailSize {
            get {
                return _viewModel.ThumbnailSize < RPlotHistoryViewModel.MaxThumbnailSize;
            }
        }

        public bool AutoHide {
            get {
                return _viewModel.AutoHide;
            }

            set {
                _viewModel.AutoHide = value;
            }
        }

        private void Control_ContextMenuRequested(object sender, MouseButtonEventArgs e) {
            Container.ShowContextMenu(RPlotCommandIds.PlotHistoryContextMenu, GetPosition(e, (FrameworkElement)sender));
        }

        private static Point GetPosition(InputEventArgs e, FrameworkElement fe) {
            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs != null) {
                return mouseEventArgs.GetPosition(fe);
            }

            var touchEventArgs = e as TouchEventArgs;
            if (touchEventArgs != null) {
                return touchEventArgs.GetTouchPoint(fe).Position;
            }

            return new Point(0, 0);
        }

        public void DecreaseThumbnailSize() {
            _viewModel.DecreaseThumbnailSize();
        }

        public void IncreaseThumbnailSize() {
            _viewModel.IncreaseThumbnailSize();
        }
    }
}
