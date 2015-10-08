using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Xaml;

namespace Microsoft.VisualStudio.R.Package.Plots
{
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
            Debug.Assert(e.NewPlotElement != null);

            this.RootContainer.Child = e.NewPlotElement;
        }
    }
}
