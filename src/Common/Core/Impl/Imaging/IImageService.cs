// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core.Imaging {
    /// <summary>
    /// Provides access to images and icons
    /// </summary>
    public interface IImageService {
        /// <summary>
        /// Provides image based on one of the predefined types
        /// </summary>
        /// <returns>Platform-specific image source</returns>
        object GetImage(ImageType imageType, ImageSubType subType = ImageSubType.Public);

        /// <summary>
        /// Returns image source given name of the image moniker
        /// such as name from http://glyphlist.azurewebsites.net/knownmonikers
        /// when running in Visual Studio.
        /// </summary>
        /// <returns>Platform-specific image source</returns>
        object GetImage(string name);

        /// <summary>
        /// Given file name returns icon depending on the file extension
        /// </summary>
        /// <returns>Platform-specific image source</returns>
        object GetFileIcon(string file);
    }
}
