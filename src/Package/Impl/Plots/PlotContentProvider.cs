using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xaml;
using Microsoft.Languages.Editor.Tasks;

namespace Microsoft.VisualStudio.R.Package.Plots {
    internal sealed class PlotContentProvider : IPlotContentProvider {
        private string _lastLoadFile;
        private string _lastIdleLoadFile;

        #region IPlotContentProvider implementation

        public event EventHandler<PlotChangedEventArgs> PlotChanged;

        public void LoadFileOnIdle(string fileName) {
            IdleTimeAction.Cancel(typeof(PlotContentProvider));
            DeleteTempFile(_lastIdleLoadFile);

            IdleTimeAction.Create(() => LoadFile(fileName), 200, typeof(PlotContentProvider));
            _lastIdleLoadFile = fileName;
        }

        public void LoadFile(string fileName) {
            UIElement element = null;
            try {
                element = (UIElement)XamlServices.Load(fileName);
                _lastLoadFile = fileName;
            } catch (Exception e) {
                element = CreateErrorContent(
                    new FormatException(string.Format("Couldn't load XAML file from {0}", fileName), e));
            }

            OnPlotChanged(element);
        }

        public void SaveFile(string fileName) {
            if (_lastLoadFile != null) {
                File.Copy(_lastLoadFile, fileName, overwrite: true);
                DeleteTempFile(_lastLoadFile);
                _lastLoadFile = null;
            }
        }

        #endregion IPlotContentProvider implementation

        private static UIElement CreateErrorContent(Exception e) {
            return new TextBlock() {
                Text = e.ToString()    // TODO: change to user-friendly error XAML. TextBlock with exception is for dev
            };
        }

        private void OnPlotChanged(UIElement element) {
            if (PlotChanged != null) {
                PlotChanged(this, new PlotChangedEventArgs() { NewPlotElement = element });
            }
        }

        private static void DeleteTempFile(string fileName) {
            if (!string.IsNullOrEmpty(fileName)) {
                try {
                    if (File.Exists(fileName)) {
                        File.Delete(fileName);
                    }
                } catch (IOException) { }
            }
        }

        public void Dispose() {
            IdleTimeAction.Cancel(typeof(PlotContentProvider));
            DeleteTempFile(_lastLoadFile);
        }
    }
}
