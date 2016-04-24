// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using Microsoft.R.Components.Plots;

namespace Microsoft.VisualStudio.R.Package.Plots {
    /// <summary>
    /// Interaction logic for XamlPresenter.xaml
    /// </summary>
    internal partial class XamlPresenter : UserControl
    {
        private readonly IPlotContentProvider _contentProvider;

        public XamlPresenter(IPlotContentProvider contentProvider)
        {
            InitializeComponent();

            _contentProvider = contentProvider;
            _contentProvider.PlotChanged += ContentProvider_PlotChanged;
        }

        private void ContentProvider_PlotChanged(object sender, PlotChangedEventArgs e)
        {
            this.RootContainer.Child = e.NewPlotElement;
            this.Watermark.Visibility = e.NewPlotElement == null ? Visibility.Visible : Visibility.Hidden;
            this.Scroller.Visibility = e.NewPlotElement == null ? Visibility.Hidden : Visibility.Visible;

            var element = e.NewPlotElement as FrameworkElement;
            if (element != null)
            {
                // Force calculate the size of the plot element
                element.UpdateLayout();

                // If it doesn't fit into our available space, display the scrollbars
                bool showScrollbars = element.ActualWidth > ActualWidth || element.ActualHeight > ActualHeight;
                Scroller.HorizontalScrollBarVisibility = Scroller.VerticalScrollBarVisibility  = showScrollbars ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
            }
        }
    }
}
