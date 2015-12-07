using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xaml;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Repl.Session;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Plots {
    internal sealed class PlotContentProvider : IPlotContentProvider {
        private IRSession _rSession;
        private string _lastLoadFile;
        private string _lastIdleLoadFile;
        private int _lastWidth;
        private int _lastHeight;

        /// <summary>
        /// R current session change triggers this SessionsChanged event
        /// </summary>
        public event EventHandler SessionsChanged;

        public PlotContentProvider() {
            _lastWidth = -1;
            _lastHeight = -1;

            var sessionProvider = AppShell.Current.ExportProvider.GetExport<IRSessionProvider>().Value;
            sessionProvider.CurrentSessionChanged += RSessionProvider_CurrentChanged;

            IdleTimeAction.Create(() => {
                SetRSession(sessionProvider.Current);
            }, 10, typeof(PlotContentProvider));
        }

        private void SetRSession(IRSession session) {
            // cleans up old RSession
            if (_rSession != null) {
                _rSession.Mutated -= RSession_Mutated;
                _rSession.Connected -= RSession_Connected;
            }

            // set new RSession
            _rSession = session;
            if (_rSession != null) {
                _rSession.Mutated += RSession_Mutated;
                _rSession.Connected += RSession_Connected;
            }

            // notify the change
            if (SessionsChanged != null) {
                SessionsChanged(this, EventArgs.Empty);
            }
        }

        private void RSession_Mutated(object sender, EventArgs e) {
        }

        private async void RSession_Connected(object sender, EventArgs e) {
            // Let the host know the size of plot window
            if (_lastWidth >= 0 && _lastHeight >= 0) {
                ResizePlot();
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken.None);

            OnPlotChanged(null);
        }

        /// <summary>
        /// IRSessionProvider.CurrentSessionChanged handler. When current session changes, this is called
        /// </summary>
        private void RSessionProvider_CurrentChanged(object sender, EventArgs e) {
            var sessionProvider = sender as IRSessionProvider;
            Debug.Assert(sessionProvider != null);

            if (sessionProvider != null) {
                SetRSession(sessionProvider.Current);
            }
        }

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
                if (string.Compare(Path.GetExtension(fileName), ".png", StringComparison.InvariantCultureIgnoreCase) == 0) {
                    var image = new Image();
                    image.Source = new BitmapImage(new Uri(fileName));
                    element = image;
                } else {
                    element = (UIElement)XamlServices.Load(fileName);
                }
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

        public async void ExportFile(string fileName) {
            if (_rSession == null) {
                return;
            }

            string device = String.Empty;
            switch (Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant()) {
                case "png":
                    device = "png";
                    break;
                case "bmp":
                    device = "bmp";
                    break;
                case "tif":
                case "tiff":
                    device = "tiff";
                    break;
                case "jpg":
                case "jpeg":
                    device = "jpeg";
                    break;
                default:
                    //TODO
                    Debug.Assert(false, "Unsupported image format.");
                    return;

            }
            using (IRSessionEvaluation eval = await _rSession.BeginEvaluationAsync()) {
                await eval.CopyToDevice(device, fileName);
            }
        }

        public async void NextPlot() {
            if (_rSession == null) {
                return;
            }

            using (IRSessionEvaluation eval = await _rSession.BeginEvaluationAsync()) {
                await eval.NextPlot();
            }
        }

        public async void PreviousPlot() {
            if (_rSession == null) {
                return;
            }

            using (IRSessionEvaluation eval = await _rSession.BeginEvaluationAsync()) {
                await eval.PreviousPlot();
            }
        }

        public void ResizePlot(int width, int height) {
            // Cache the size, so we can set the initial size
            // whenever we get a new session
            _lastWidth = width;
            _lastHeight = height;

            if (_rSession != null) {
                ResizePlot();
            }
        }

        private async void ResizePlot() {
            if (_rSession != null) {
                using (IRSessionEvaluation eval = await _rSession.BeginEvaluationAsync()) {
                    await eval.ResizePlot(_lastWidth, _lastHeight);
                }
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
