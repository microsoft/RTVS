using System;
using System.Windows.Media.Imaging;
using Microsoft.R.Wpf.ValueConverters;

namespace Microsoft.R.Components {
    internal static class Images {
        public static readonly BitmapImage DefaultPackageIcon;

        static Images() {
            DefaultPackageIcon = new BitmapImage();
            DefaultPackageIcon.BeginInit();

            // If the DLL name changes, this URI would need to change to match.
            DefaultPackageIcon.UriSource = new Uri("pack://application:,,,/Microsoft.R.Components.Windows;component/Resources/packageicon.png");

            // Instead of scaling larger images and keeping larger image in memory, this makes it so we scale it down, and throw away the bigger image.
            // Only need to set this on one dimension, to preserve aspect ratio
            DefaultPackageIcon.DecodePixelWidth = 32;

            DefaultPackageIcon.EndInit();
            DefaultPackageIcon.Freeze();

            IconUrlToImageCacheConverter.DefaultPackageIcon = DefaultPackageIcon;
        }
    }
}
