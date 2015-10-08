using System;
using System.Windows;
using System.Windows.Controls;
using System.Xaml;

namespace Microsoft.VisualStudio.R.Package.Plots
{
    internal class PlotContentProvider : IPlotContentProvider
    {
        #region IPlotContentProvider implementation

        public event EventHandler<PlotChangedEventArgs> PlotChanged;

        public void LoadFile(string fileName)
        {
            UIElement element = null;
            try
            {
                element = (UIElement) XamlServices.Load(fileName);
            }
            catch (Exception e)
            {
                element = CreateErrorContent(
                    new FormatException(string.Format("Couldn't load XAML file from {0}", fileName), e));
            }

            OnPlotChanged(element);
        }

        public void LoadXaml(string xamlText)
        {
            UIElement element = null;
            try
            {
                element = (UIElement)XamlServices.Parse(xamlText);
            }
            catch (Exception e)
            {
                element = CreateErrorContent(e);
            }

            OnPlotChanged(element);
        }

        #endregion IPlotContentProvider implementation

        private static UIElement CreateErrorContent(Exception e)
        {
            return new TextBlock() {
                Text = e.ToString()    // TODO: change to user-friendly error XAML. TextBlock with exception is for dev
            };
        }

        private void OnPlotChanged(UIElement element)
        {
            if (PlotChanged != null)
            {
                PlotChanged(this, new PlotChangedEventArgs() { NewPlotElement = element });
            }
        }
    }
}
