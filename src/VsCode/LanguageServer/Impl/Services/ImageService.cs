// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using LanguageServer.VsCode.Contracts;
using Microsoft.Common.Core.Imaging;

namespace Microsoft.R.LanguageServer.Services {
    internal sealed class ImageService: IImageService {
        public object GetImage(ImageType imageType, ImageSubType subType = ImageSubType.Public) {
            switch(imageType) {
                case ImageType.Constant:
                    return CompletionItemKind.Enum;
                case ImageType.OpenFolder:
                case ImageType.ClosedFolder:
                case ImageType.File:
                case ImageType.Document:
                    return CompletionItemKind.File;
                case ImageType.Intrinsic:
                    return CompletionItemKind.Property;
                case ImageType.Keyword:
                    return CompletionItemKind.Keyword;
                case ImageType.Library:
                    return CompletionItemKind.Module;
                case ImageType.Method:
                    return CompletionItemKind.Method;
                case ImageType.Snippet:
                    return CompletionItemKind.Snippet;
                case ImageType.ValueType:
                    return CompletionItemKind.Value;
                case ImageType.Variable:
                    return CompletionItemKind.Variable;
            }
            return null;
        }

        public object GetImage(string name) => null;

        public object GetFileIcon(string file) => null;
    }
}
