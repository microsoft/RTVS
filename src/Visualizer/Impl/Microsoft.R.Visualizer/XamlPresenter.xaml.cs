using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Xaml;

namespace Microsoft.R.Visualizer
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

        public void LoadXaml(string xamlText)
        {
            object parsed = null;

            try
            {
                parsed = XamlServices.Parse(xamlText);
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
