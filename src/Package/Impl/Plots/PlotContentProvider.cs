// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xaml;
using Microsoft.Common.Core;
using Microsoft.R.Components.Plots;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Plots {
    internal sealed class PlotContentProvider : IPlotContentProvider {
        private IRSession _rSession;
        private int _lastPixelWidth;
        private int _lastPixelHeight;
        private int _lastResolution;

        public PlotContentProvider(IRSession session) {
            _lastPixelWidth = -1;
            _lastPixelHeight = -1;
            _lastResolution = -1;

            _rSession = session;
            _rSession.Connected += RSession_Connected;
        }

        private async void RSession_Connected(object sender, EventArgs e) {
            // Let the host know the size of plot window
            if (_lastPixelWidth >= 0 && _lastPixelHeight >= 0 && _lastResolution >= 0) {
                try {
                    await ApplyNewSize();
                } catch (OperationCanceledException) { } catch (MessageTransportException) { }
            }

            VsAppShell.Current.DispatchOnUIThread(() => {
                OnPlotChanged(null);
            });
        }

        #region IPlotContentProvider implementation

        public IPlotLocator Locator { get; set; }

        public event EventHandler<PlotChangedEventArgs> PlotChanged;

        public void LoadFile(string fileName) {
            UIElement element = null;
            // Empty filename means clear
            if (!string.IsNullOrEmpty(fileName)) {
                try {
                    if (string.Compare(Path.GetExtension(fileName), ".png", StringComparison.OrdinalIgnoreCase) == 0) {
                        var fileInfo = new FileInfo(fileName);
                        if (fileInfo.Length > 0) {
                            element = CreateBitmapContent(fileName);
                        } else {
                            // A zero-sized .png file means a blank image
                            element = CreateBlankContent();
                        }
                    } else {
                        element = CreateXamlContent(fileName);
                    }
                } catch (Exception e) when (!e.IsCriticalException()) {
                    element = CreateErrorContent(
                        new FormatException(string.Format(CultureInfo.InvariantCulture, Resources.PlotLoadError, fileName), e));
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
                    await eval.ExportToBitmap(deviceName, fileName, _lastPixelWidth, _lastPixelHeight, _lastResolution);
                }
            }
        }

        private async System.Threading.Tasks.Task CopyToClipboardAsBitmapAsync() {
            if (_rSession != null) {
                string fileName = Path.GetTempFileName();
                using (IRSessionEvaluation eval = await _rSession.BeginEvaluationAsync()) {
                    await eval.ExportToBitmap("bmp", fileName, _lastPixelWidth, _lastPixelHeight, _lastResolution);
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
                            } catch (Exception e) when (!e.IsCriticalException()) {
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
                    await eval.ExportToMetafile(fileName, PixelsToInches(_lastPixelWidth), PixelsToInches(_lastPixelHeight), _lastResolution);

                    VsAppShell.Current.DispatchOnUIThread(
                        () => {
                            try {
                                var mf = new System.Drawing.Imaging.Metafile(fileName);
                                Clipboard.SetData(DataFormats.EnhancedMetafile, mf);

                                SafeFileDelete(fileName);
                            } catch (Exception e) when (!e.IsCriticalException()) {
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
                    await eval.ExportToPdf(fileName, PixelsToInches(_lastPixelWidth), PixelsToInches(_lastPixelHeight));
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

        public async System.Threading.Tasks.Task ClearAllAsync() {
            if (_rSession == null) {
                return;
            }

            using (var eval = await _rSession.BeginInteractionAsync(false)) {
                await eval.ClearPlotHistory();
            }
        }

        public async System.Threading.Tasks.Task RemoveCurrentPlotAsync() {
            if (_rSession == null) {
                return;
            }

            using (var eval = await _rSession.BeginInteractionAsync(false)) {
                await eval.RemoveCurrentPlot();
            }
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

        public async System.Threading.Tasks.Task ResizePlotAsync(int pixelWidth, int pixelHeight, int resolution) {
            // Cache the size, so we can set the initial size
            // whenever we get a new session
            _lastPixelWidth = pixelWidth;
            _lastPixelHeight = pixelHeight;
            _lastResolution = resolution;

            if (_rSession != null) {
                await ApplyNewSize();
            }
        }

        private async System.Threading.Tasks.Task ApplyNewSize() {
            if (_rSession != null) {
                using (var eval = await _rSession.BeginInteractionAsync(false)) {
                    await eval.ResizePlot(_lastPixelWidth, _lastPixelHeight, _lastResolution);
                }
            }
        }

        #endregion IPlotContentProvider implementation

        private static UIElement CreateErrorContent(Exception e) {
            return new TextBlock() {
                Text = e.Message
            };
        }

        private static UIElement CreateBitmapContent(string fileName) {
            // Use Begin/EndInit to avoid locking the file on disk
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(fileName);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();

            var image = new NonScaledImage();
            image.Source = bmp;

            return image;
        }

        private static UIElement CreateXamlContent(string fileName) {
            return (UIElement)XamlServices.Load(fileName);
        }

        private static UIElement CreateBlankContent() {
            return new TextBlock();
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
            task.SilenceException<MessageTransportException>()
                .DoNotWait();
        }

        //public void Locator(CancellationToken ct, Action<LocatorResult> doneCallback) {
        //    if (_locatorHandler != null) {
        //        _locatorDoneCallback = doneCallback;
        //        _locatorHandler(LocatorDoneCallback);
        //    }

        //    if (_locatorEndHandler != null) {
        //        ct.Register(() => EndLocator());
        //    }
        //}

        //private void LocatorDoneCallback(LocatorResult result) {
        //    if (_locatorDoneCallback != null) {
        //        _locatorDoneCallback(result);
        //        _locatorDoneCallback = null;
        //    }

        //    _locatorEndHandler?.Invoke();
        //}

        //public void SetLocatorHandler(Action<Action<LocatorResult>> handler, Action endHandler) {
        //    _locatorHandler = handler;
        //    _locatorEndHandler = endHandler;
        //}

        //public void EndLocator() {
        //    LocatorDoneCallback(new LocatorResult());
        //}

        //public bool IsInLocatorMode {
        //    get { return _locatorDoneCallback != null; }
        //}
    }

    internal static class WpfUnitsConversion {
        public static Size FromPixels(Visual visual, Size pixelSize) {
            var source = PresentationSource.FromVisual(visual);
            return (Size)source.CompositionTarget.TransformFromDevice.Transform((Vector)pixelSize);
        }

        public static Size ToPixels(Visual visual, Size wpfSize) {
            var source = PresentationSource.FromVisual(visual);
            return (Size)source.CompositionTarget.TransformToDevice.Transform((Vector)wpfSize);
        }

        public static Point ToPixels(Visual visual, Point wpfSize) {
            var source = PresentationSource.FromVisual(visual);
            return (Point)source.CompositionTarget.TransformToDevice.Transform((Vector)wpfSize);
        }

        public static int GetResolution(Visual visual) {
            var source = PresentationSource.FromVisual(visual);
            int res = (int)(96 * source.CompositionTarget.TransformToDevice.M11);
            return res;
        }
    }

    /// <summary>
    /// An bitmap image that is rendered to the screen without being scaled,
    /// where each pixel in the bitmap takes one physical pixel on screen.
    /// </summary>
    internal class NonScaledImage : Image {
        protected override Size MeasureOverride(Size constraint) {
            BitmapImage bmp = this.Source as BitmapImage;
            if (bmp != null) {
                // WPF assumes that your code doesn't have special dpi handling
                // and automatically sizes bitmaps to avoid them being too
                // small when running at high dpi.
                // We prevent that scaling by calculating a size based on
                // pixel size and dpi setting.
                Size bitmapSize = new Size(bmp.PixelWidth, bmp.PixelHeight);
                return WpfUnitsConversion.FromPixels(this, bitmapSize);
            }
            return base.MeasureOverride(constraint);
        }
    }
}
