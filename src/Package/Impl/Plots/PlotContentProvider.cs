using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xaml;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.R.Debugger;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Plots {
    internal sealed class PlotContentProvider : IPlotContentProvider {
        private IRSession _rSession;
        private IDebugSessionProvider _debugSessionProvider;
        private string _lastLoadFile;
        private int _lastWidth;
        private int _lastHeight;

        /// <summary>
        /// R current session change triggers this SessionsChanged event
        /// </summary>
        public event EventHandler SessionsChanged;

        public PlotContentProvider() {
            _lastWidth = -1;
            _lastHeight = -1;

            var sessionProvider = VsAppShell.Current.ExportProvider.GetExport<IRSessionProvider>().Value;
            sessionProvider.CurrentSessionChanged += RSessionProvider_CurrentChanged;

            _debugSessionProvider = VsAppShell.Current.ExportProvider.GetExport<IDebugSessionProvider>().Value;

            IdleTimeAction.Create(() => {
                SetRSession(sessionProvider.Current).DoNotWait();
            }, 10, typeof(PlotContentProvider));
        }

        private async System.Threading.Tasks.Task SetRSession(IRSession session) {
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

                // debug session is created to trigger a load of the R package
                // that has functions we need such as rtvs:::toJSON
                var debugSession = await _debugSessionProvider.GetDebugSessionAsync(_rSession);
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
                await ApplyNewSize();
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
                SetRSession(sessionProvider.Current).DoNotWait();
            }
        }

        #region IPlotContentProvider implementation

        public event EventHandler<PlotChangedEventArgs> PlotChanged;

        public void LoadFile(string fileName) {
            UIElement element = null;
            // Empty filename means clear
            if (!string.IsNullOrEmpty(fileName)) {
                try {
                    if (string.Compare(Path.GetExtension(fileName), ".png", StringComparison.InvariantCultureIgnoreCase) == 0) {
                        var image = new Image();
                        image.Source = new BitmapImage(new Uri(fileName));
                        element = image;
                    } else {
                        element = (UIElement)XamlServices.Load(fileName);
                    }
                    _lastLoadFile = fileName;
                } catch (Exception e) when (!e.IsCriticalException()) {
                    element = CreateErrorContent(
                        new FormatException(string.Format("Couldn't load XAML file from {0}", fileName), e));
                }
            }

            OnPlotChanged(element);
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

        public async Task<PlotHistoryInfo> GetHistoryInfoAsync() {
            if (_rSession == null || !_rSession.IsHostRunning) {
                return new PlotHistoryInfo();
            }

            REvaluationResult result;
            using (IRSessionEvaluation eval = await _rSession.BeginEvaluationAsync()) {
                result = await eval.PlotHistoryInfo();
            }

            return new PlotHistoryInfo(
                (int)result.JsonResult[0].ToObject(typeof(int)),
                (int)result.JsonResult[1].ToObject(typeof(int)));
        }

        public async System.Threading.Tasks.Task NextPlotAsync() {
            if (_rSession == null) {
                return;
            }

            using (var eval = await _rSession.BeginInteractionAsync(false)) {
                await eval.NextPlot();
            }
        }

        public async System.Threading.Tasks.Task PreviousPlotAsync() {
            if (_rSession == null) {
                return;
            }

            using (var eval = await _rSession.BeginInteractionAsync(false)) {
                await eval.PreviousPlot();
            }
        }

        public async System.Threading.Tasks.Task ResizePlotAsync(int width, int height) {
            // Cache the size, so we can set the initial size
            // whenever we get a new session
            _lastWidth = width;
            _lastHeight = height;

            if (_rSession != null) {
                await ApplyNewSize();
            }
        }

        private async System.Threading.Tasks.Task ApplyNewSize() {
            if (_rSession != null) {
                using (var eval = await _rSession.BeginInteractionAsync(false)) {
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

        public void Dispose() {
        }
    }
}
