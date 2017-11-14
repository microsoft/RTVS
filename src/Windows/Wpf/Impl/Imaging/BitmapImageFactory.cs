// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Microsoft.Common.Wpf.Imaging {
    public static class BitmapImageFactory {
        /// <summary>
        /// Load and immediately initialie a <see cref="BitmapImage"/> from
        /// a file on disk. This does not keep a lock on the file.
        /// </summary>
        public static BitmapImage Load(string filePath) {
            // Use Begin/EndInit to avoid locking the file on disk
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(filePath);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            return image;
        }

        /// <summary>
        /// Load and immediately initialize a <see cref="BitmapImage"/> from
        /// a stream.
        /// </summary>
        public static BitmapImage Load(Stream stream) {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            return image;
        }
    }
}
