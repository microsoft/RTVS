using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xaml;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tasks;

namespace Microsoft.VisualStudio.R.Package.Plots
{
    internal class PlotContentProvider : IPlotContentProvider
    {
        private string _lastLoadFile;

        #region IPlotContentProvider implementation

        public event EventHandler<PlotChangedEventArgs> PlotChanged;

        public void LoadFileOnIdle(string fileName)
        {
            IdleTimeAction.Cancel(typeof(PlotContentProvider));
            IdleTimeAction.Create(() => LoadFile(fileName), 200, typeof(PlotContentProvider));
        }

        public void LoadFile(string fileName)
        {
            UIElement element = null;
            try
            {
                element = (UIElement)XamlServices.Load(fileName);
                _lastLoadFile = fileName;
            }
            catch (Exception e)
            {
                element = CreateErrorContent(
                    new FormatException(string.Format("Couldn't load XAML file from {0}", fileName), e));
            }

            OnPlotChanged(element);
        }

        public void SaveFile(string fileName)
        {
            if (_lastLoadFile != null)
            {
                File.Copy(_lastLoadFile, fileName, true);   // overwrite
            }
        }

        #endregion IPlotContentProvider implementation

        private static UIElement CreateErrorContent(Exception e)
        {
            return new TextBlock()
            {
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
