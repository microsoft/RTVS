// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.IO;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Utilities {
    public static class PathExtensions {
        public static string ToShortRelativePath(this IFileSystem fileSystem, string path, string rootFolder) {
            var shortPath = fileSystem.ToShortPath(path);
            var rootShortPath = fileSystem.ToShortPath(rootFolder);
            return PathHelper.MakeRelative(rootShortPath, shortPath);
        }
    }
}
