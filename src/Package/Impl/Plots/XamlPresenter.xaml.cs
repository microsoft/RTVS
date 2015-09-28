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
    public partial class XamlPresenter : UserControl
    {
        public XamlPresenter()
        {
            InitializeComponent();
        }

        public void LoadXamlFile(string fileName)
        {
            SetContainer(() => XamlServices.Load(fileName));
        }

        public void LoadXaml(string xamlText)
        {
            SetContainer(() => XamlServices.Parse(xamlText));
        }

        private void SetContainer(Func<object> xamlObjectLoader)
        {
            object parsed = null;

            try
            {
                parsed = xamlObjectLoader();
            }
            catch (Exception e)
            {
                parsed = new TextBlock() { Text = string.Format("Couldn't parse XAML \r\n{0}", e.ToString()) };
            }

            var parsedObj = parsed as UIElement;
            Debug.Assert(parsedObj != null);

            this.RootContainer.Child = parsedObj;
        }
    }
}
