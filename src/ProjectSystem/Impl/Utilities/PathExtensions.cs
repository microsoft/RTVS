// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Interop;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Utilities {
    public static class PathExtensions {
        public static string ToLongPath(this string path) {
            var sb = new StringBuilder(NativeMethods.MAX_PATH);
            NativeMethods.GetLongPathName(path, sb, sb.Capacity);
            return sb.ToString();
        }

        public static string ToShortPath(this string path) {
            var sb = new StringBuilder(NativeMethods.MAX_PATH);
            NativeMethods.GetShortPathName(path, sb, sb.Capacity);
            return sb.ToString();
        }

        public static string ToShortRelativePath(this string path, string rootFolder) {
            return PathHelper.MakeRelative(path.ToShortPath(), rootFolder.ToShortPath());
        }
    }
}
