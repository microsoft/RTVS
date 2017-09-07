// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Imaging;

namespace Microsoft.R.LanguageServer.Services {
    internal sealed class ImageService: IImageService {
        public object GetImage(ImageType imageType, ImageSubType subType = ImageSubType.Public) => null;

        public object GetImage(string name) => null;

        public object GetFileIcon(string file) => null;
    }
}
