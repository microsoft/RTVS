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
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Plots {
    internal sealed class PlotContentProvider : IPlotContentProvider {
        private IRSession _rSession;
        private IDebugSessionProvider _debugSessionProvider;
        private string _lastLoadFile;
        private int _lastWidth;
        private int _lastHeight;

        public PlotContentProvider() {
            _lastWidth = -1;
            _lastHeight = -1;

            var sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            _rSession = sessionProvider.GetInteractiveWindowRSession();
            _rSession.Mutated += RSession_Mutated;
            _rSession.Connected += RSession_Connected;

            _debugSessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IDebugSessionProvider>();

            IdleTimeAction.Create(() => {
                // debug session is created to trigger a load of the R package
                // that has functions we need such as rtvs:::toJSON
                _debugSessionProvider.GetDebugSessionAsync(_rSession).DoNotWait();
            }, 10, typeof(PlotContentProvider));
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

        #region IPlotContentProvider implementation

        public event EventHandler<PlotChangedEventArgs> PlotChanged;

        public void LoadFile(string fileName) {
            UIElement element = null;
            // Empty filename means clear
            if (!string.IsNullOrEmpty(fileName)) {
                try {
                    if (string.Compare(Path.GetExtension(fileName), ".png", StringComparison.InvariantCultureIgnoreCase) == 0) {
                        // Use Begin/EndInit to avoid locking the file on disk
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri(fileName);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();

                        var image = new Image();
                        image.Source = bmp;

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

        public void ExportAsImage(string fileName, string deviceName) {
            DoNotWait(ExportAsImageAsync(fileName, deviceName));
        }

        public void CopyToClipboardAsBitmap() {
            DoNotWait(CopyToClipboardAsBitmapAsync());
        }

        public void CopyToClipboardAsMetafile() {
            DoNotWait(CopyToClipboardAsMetafileAsync());
        }

        public void ExportAsPdf(string fileName) {
            DoNotWait(ExportAsPdfAsync(fileName));
        }

        private async System.Threading.Tasks.Task ExportAsImageAsync(string fileName, string deviceName) {
            if (_rSession != null) {
                using (IRSessionEvaluation eval = await _rSession.BeginEvaluationAsync()) {
                    await eval.ExportToBitmap(deviceName, fileName, _lastWidth, _lastHeight);
                }
            }
        }

        private async System.Threading.Tasks.Task CopyToClipboardAsBitmapAsync() {
            if (_rSession != null) {
                string fileName = Path.GetTempFileName();
                using (IRSessionEvaluation eval = await _rSession.BeginEvaluationAsync()) {
                    await eval.ExportToBitmap("bmp", fileName, _lastWidth, _lastHeight);
                    VsAppShell.Current.DispatchOnUIThread(
                        () => {
                            try {
                            // Use Begin/EndInit to avoid locking the file on disk
                            var image = new BitmapImage();
                                image.BeginInit();
                                image.UriSource = new Uri(fileName);
                                image.CacheOption = BitmapCacheOption.OnLoad;
                                image.EndInit();
                                Clipboard.SetImage(image);

                                SafeFileDelete(fileName);
                            } catch (IOException e) {
                                MessageBox.Show(string.Format(Resources.PlotCopyToClipboardError, e.Message));
                            }
                        });
                }
            }
        }

        private async System.Threading.Tasks.Task CopyToClipboardAsMetafileAsync() {
            if (_rSession != null) {
                string fileName = Path.GetTempFileName();
                using (IRSessionEvaluation eval = await _rSession.BeginEvaluationAsync()) {
                    await eval.ExportToMetafile(fileName, PixelsToInches(_lastWidth), PixelsToInches(_lastHeight));

                    VsAppShell.Current.DispatchOnUIThread(
                        () => {
                            try {
                                var mf = new System.Drawing.Imaging.Metafile(fileName);
                                Clipboard.SetData(DataFormats.EnhancedMetafile, mf);

                                SafeFileDelete(fileName);
                            } catch (IOException e) {
                                MessageBox.Show(string.Format(Resources.PlotCopyToClipboardError, e.Message));
                            }
                        });
                }
            }
        }

        private static double PixelsToInches(int pixels) {
            return pixels / 96.0;
        }

        private static void SafeFileDelete(string fileName) {
            try {
                File.Delete(fileName);
            } catch (IOException) {
            }
        }

        private async System.Threading.Tasks.Task ExportAsPdfAsync(string fileName) {
            if (_rSession != null) {
                using (IRSessionEvaluation eval = await _rSession.BeginEvaluationAsync()) {
                    await eval.ExportToPdf(fileName, PixelsToInches(_lastWidth), PixelsToInches(_lastHeight), "special");
                }
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

        public static void DoNotWait(System.Threading.Tasks.Task task) {
            // Errors like invalid graphics state which go to the REPL stderr will come back
            // in an Microsoft.R.Host.Client.RException, and we don't need to do anything with them,
            // as the user can see them in the REPL.
            // TODO:
            // See if we can fix the cause of those errors - to be
            // determined based on the various errors we see displayed
            // in REPL during testing.
            task.SilenceException<MessageTransportException>()
                .SilenceException<Microsoft.R.Host.Client.RException>()
                .DoNotWait();
        }
    }
}
