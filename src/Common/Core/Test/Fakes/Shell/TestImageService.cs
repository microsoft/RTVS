// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Media;
using Microsoft.Common.Core.Imaging;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    public sealed class TestImageService : IImageService {
        private readonly ImageSource _image;

        public TestImageService() {
            _image = new DrawingImage(new GeometryDrawing(
                Brushes.Blue,
                new Pen(Brushes.Transparent, 0),
                new RectangleGeometry(new Rect(0, 0, 16, 16))
            ));
        }

        public object GetFileIcon(string file) => _image;
        public object GetImage(ImageType imageType) => _image;
        public object GetImage(string name) => _image;
    }
}
