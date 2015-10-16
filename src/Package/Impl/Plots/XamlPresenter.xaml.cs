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

        private UIElement _watermarkElement;

        public XamlPresenter(IPlotContentProvider contentProvider)
        {
            InitializeComponent();

            SetWatermark();

            _contentProvider = contentProvider;
            _contentProvider.PlotChanged += ContentProvider_PlotChanged;
        }

        public UIElement PlotElement
        {
            get
            {
                return this.RootContainer.Child;
            }
        }

        private void SetWatermark()
        {
            if (_watermarkElement == null)
            {
                // TODO: define this in XAML file. why dynamic creation? you lazy boy
                _watermarkElement = (UIElement)XamlServices.Parse(
                    "<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
                    "Foreground=\"DarkGray\" " +
                    "TextAlignment=\"Center\" " +
                    "VerticalAlignment=\"Center\" " +
                    "HorizontalAlignment=\"Center\" " +
                    "TextWrapping=\"Wrap\">" +
                    Package.Resources.EmptyPlotWindowWatermark +
                    "</TextBlock>");
            }
            this.RootContainer.Child = _watermarkElement;

            //XamlServices.Save(this.RootContainer.Child);
        }

        private void ContentProvider_PlotChanged(object sender, PlotChangedEventArgs e)
        {
            if (e.NewPlotElement == null)
            {
                SetWatermark();
            }
            else
            {
                this.RootContainer.Child = e.NewPlotElement;
            }
        }
    }
}
