// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Common.Core.Imaging;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    internal sealed class TestImageService : IImageService {
        private readonly ImageSource _image;

        public TestImageService() {
            using (var memory = new MemoryStream()) {
                Resources.BlueSquare.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = memory;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                _image = image;
            }
        }

        public object GetFileIcon(string file) => _image;
        public object GetImage(ImageType imageType) => _image;
        public object GetImage(string name) => _image;
    }
}
