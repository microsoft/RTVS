// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core {
    public static class IOExtensions {
        public static string MakeRelativePath(this string path, string basePath) {
            if (!basePath.EndsWith("\\")) {
                basePath += "\\";
            }
            if (path.StartsWithIgnoreCase(basePath)) {
                return path.Substring(basePath.Length);
            }
            return path;
        }
    }
}
