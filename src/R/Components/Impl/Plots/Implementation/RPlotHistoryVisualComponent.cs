// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Input;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Plots.Implementation.View;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.View;

namespace Microsoft.R.Components.Plots.Implementation {
    public class RPlotHistoryVisualComponent : IRPlotHistoryVisualComponent {
        private readonly DisposableBag _disposableBag;
        private ICoreShell Shell { get; }
        private IRPlotManager PlotManager { get; }
        public IRPlotHistoryViewModel ViewModel { get; }

        public RPlotHistoryVisualComponent(IRPlotManager plotManager, ICommandTarget controller, IRPlotHistoryViewModel viewModel, IVisualComponentContainer<IRPlotHistoryVisualComponent> container, IRSettings settings, ICoreShell coreShell) {
            PlotManager = plotManager;
            Controller = controller;
            ViewModel = viewModel;
            Container = container;
            Shell = coreShell;

            var control = new RPlotHistoryControl {
                DataContext = ViewModel
            };

            Control = control;

            _disposableBag = DisposableBag.Create<RPlotDeviceVisualComponent>()
                .Add(() => control.ContextMenuRequested -= Control_ContextMenuRequested);

            control.ContextMenuRequested += Control_ContextMenuRequested;
        }

        public void Dispose() {
            _disposableBag.TryMarkDisposed();
        }

        public ICommandTarget Controller { get; }

        public FrameworkElement Control { get; }

        public IVisualComponentContainer<IVisualComponent> Container { get; }

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
    }
}
