// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Runtime.Caching;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Microsoft.R.Wpf.ValueConverters {
    public class IconUrlToImageCacheConverter : IValueConverter {
        private const int _decodePixelWidth = 32;

        // same URIs can reuse the bitmapImage that we've already used.
        private static readonly ObjectCache _bitmapImageCache = MemoryCache.Default;

        private static readonly WebExceptionStatus[] _fatalErrors = {
            WebExceptionStatus.ConnectFailure,
            WebExceptionStatus.RequestCanceled,
            WebExceptionStatus.ConnectionClosed,
            WebExceptionStatus.Timeout,
            WebExceptionStatus.UnknownError
        };

        private static readonly RequestCachePolicy _requestCacheIfAvailable = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable);
        private static readonly ErrorFloodGate _errorFloodGate = new ErrorFloodGate();

        public static BitmapImage DefaultPackageIcon { get; set; }

        // We bind to a BitmapImage instead of a Uri so that we can control the decode size, since we are displaying 32x32 images, while many of the images are 128x128 or larger.
        // This leads to a memory savings.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var iconUrl = value as Uri;
            var defaultPackageIcon = parameter as BitmapImage;
            if (iconUrl == null) {
                return null;
            }

            var iconBitmapImage = _bitmapImageCache.Get(iconUrl.ToString()) as BitmapImage;
            if (iconBitmapImage != null) {
                return iconBitmapImage;
            }

            // Some people run on networks with internal NuGet feeds, but no access to the package images on the internet.
            // This is meant to detect that kind of case, and stop spamming the network, so the app remains responsive.
            if (_errorFloodGate.IsOpen) {
                return null;
            }

            iconBitmapImage = new BitmapImage();
            iconBitmapImage.BeginInit();
            iconBitmapImage.UriSource = iconUrl;

            // Default cache policy: Per MSDN, satisfies a request for a resource either by using the cached copy of the resource or by sending a request
            // for the resource to the server. The action taken is determined by the current cache policy and the age of the content in the cache.
            // This is the cache level that should be used by most applications.
            iconBitmapImage.UriCachePolicy = _requestCacheIfAvailable;

            // Instead of scaling larger images and keeping larger image in memory, this makes it so we scale it down, and throw away the bigger image.
            // Only need to set this on one dimension, to preserve aspect ratio
            iconBitmapImage.DecodePixelWidth = _decodePixelWidth;

            iconBitmapImage.DecodeFailed += IconBitmapImage_DownloadOrDecodeFailed;
            iconBitmapImage.DownloadFailed += IconBitmapImage_DownloadOrDecodeFailed;
            iconBitmapImage.DownloadCompleted += IconBitmapImage_DownloadCompleted;

            try {
                iconBitmapImage.EndInit();
            }
            // if the URL is a file: URI (which actually happened!), we'll get an exception.
            // if the URL is a file: URI which is in an existing directory, but the file doesn't exist, we'll fail silently.
            catch (Exception e) when (e is System.IO.IOException || e is WebException) {
                iconBitmapImage = defaultPackageIcon;
            } finally {
                // store this bitmapImage in the bitmap image cache, so that other occurances can reuse the BitmapImage
                AddToCache(iconUrl, iconBitmapImage);
                _errorFloodGate.ReportAttempt();
            }

            return iconBitmapImage;
        }

        private static void AddToCache(Uri iconUrl, BitmapImage iconBitmapImage) {
            var policy = new CacheItemPolicy {
                SlidingExpiration = TimeSpan.FromMinutes(10),
                RemovedCallback = CacheEntryRemoved
            };
            _bitmapImageCache.Set(iconUrl.ToString(), iconBitmapImage, policy);
        }

        private static void CacheEntryRemoved(CacheEntryRemovedArguments arguments) {

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

        private void IconBitmapImage_DownloadCompleted(object sender, EventArgs e) {
            var bitmapImage = sender as BitmapImage;
            if (bitmapImage != null && !bitmapImage.IsFrozen) {
                bitmapImage.Freeze();
            }
        }

        private void IconBitmapImage_DownloadOrDecodeFailed(object sender, System.Windows.Media.ExceptionEventArgs e) {
            var bitmapImage = sender as BitmapImage;

            if (bitmapImage == null) {
                return;
            }

            // Fix the bitmap image cache to have default package icon, if some other failure didn't already do that.
            var iconBitmapImage = _bitmapImageCache.Get(bitmapImage.UriSource.ToString()) as BitmapImage;
            if (Equals(iconBitmapImage, DefaultPackageIcon)) {
                return;
            }

            AddToCache(bitmapImage.UriSource, DefaultPackageIcon);

            var webex = e.ErrorException as WebException;
            if (webex != null && _fatalErrors.Any(c => webex.Status == c)) {
                _errorFloodGate.ReportError();
            }
        }

        private class ErrorFloodGate {
            // If we fail at least this high (failures/attempts), we'll shut off image loads.
            private const double _stopLoadingThreshold = 0.50;
            private const int _slidingExpirationInMinutes = 60;
            private const int _minFailuresCount = 5;
            private const int _secondsInOneTick = 5;
            private readonly DateTimeOffset _origin = DateTimeOffset.Now;
            private readonly Queue<int> _attempts = new Queue<int>();
            private readonly Queue<int> _failures = new Queue<int>();

            private DateTimeOffset _lastEvaluate = DateTimeOffset.Now;
            private bool _isOpen;

            public bool IsOpen {
                get {
                    if (GetTicks(_lastEvaluate) <= 1) {
                        return _isOpen;
                    }

                    var discardOlderThan1Hour = GetTicks(DateTimeOffset.Now.AddMinutes(-_slidingExpirationInMinutes));

                    ExpireOlderValues(_attempts, discardOlderThan1Hour);
                    ExpireOlderValues(_failures, discardOlderThan1Hour);

                    var attemptsCount = _attempts.Count;
                    var failuresCount = _failures.Count;
                    _isOpen = attemptsCount > 0 && failuresCount > _minFailuresCount && ((double)failuresCount / attemptsCount) > _stopLoadingThreshold;
                    _lastEvaluate = DateTimeOffset.Now;
                    return _isOpen;
                }
            }

            private static void ExpireOlderValues(Queue<int> q, int expirationOffsetInTicks) {
                while (q.Count > 0 && q.Peek() < expirationOffsetInTicks) {
                    q.Dequeue();
                }
            }

            public void ReportAttempt() {
                var ticks = GetTicks(_origin);
                _attempts.Enqueue(ticks);
            }

            public void ReportError() {
                var ticks = GetTicks(_origin);
                _failures.Enqueue(ticks);
            }

            // Ticks here are of 5sec long
            private static int GetTicks(DateTimeOffset origin) {
                return (int)((DateTimeOffset.Now - origin).TotalSeconds / _secondsInOneTick);
            }
        }
    }
}
