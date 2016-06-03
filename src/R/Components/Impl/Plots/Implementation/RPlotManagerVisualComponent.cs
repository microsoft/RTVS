// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Plots.Implementation.View;
using Microsoft.R.Components.Plots.Implementation.ViewModel;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Plots.Implementation {
    public class RPlotManagerVisualComponent : IRPlotManagerVisualComponent {
        private IRPlotManagerViewModel ViewModel { get; }

        public RPlotManagerVisualComponent(IRPlotManager plotManager, IVisualComponentContainer<IRPlotManagerVisualComponent> container, IRSession session, IRSettings settings, ICoreShell coreShell) {
            Container = container;
            Controller = null;
            ViewModel = new RPlotManagerViewModel(plotManager);
            Control = new RPlotManagerControl() {
                DataContext = ViewModel,
            };
        }

        public void LoadPlot(BitmapImage image) {
            ViewModel.LoadPlot(image);
        }

        public void Clear() {
            ViewModel.Clear();
        }

        public void Error() {
            ViewModel.Error();
        }

        public void SetLocatorMode(bool locatorMode) {
            ViewModel.SetLocatorMode(locatorMode);
            Container.CaptionText = locatorMode ? Resources.Plots_WindowCaptionLocatorActive : Resources.Plots_WindowCaption;
            Container.StatusText = locatorMode ? Resources.Plots_StatusLocatorActive : string.Empty;
        }

        public void Click(int pixelX, int pixelY) {
            ViewModel.ClickPlot(pixelX, pixelY);
        }

        public void Dispose() {
        }

        public ICommandTarget Controller { get; }

        public FrameworkElement Control { get; }

        public IVisualComponentContainer<IVisualComponent> Container { get; }
    }
}
