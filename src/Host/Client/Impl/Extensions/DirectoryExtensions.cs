// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using Microsoft.Common.Core;

namespace Microsoft.R.Host.Client.Extensions {
    public static class DirectoryExtensions {
        public static string MakeRRelativePath(this string path, string basePath ) {
            if (!string.IsNullOrEmpty(basePath)) {
                if (path.StartsWithIgnoreCase(basePath)) {
                    var relativePath = path.MakeRelativePath(basePath);
                    if (relativePath.Length > 0) {
                        return "~/" + relativePath.Replace('\\', '/');
                    }
                    return "~";
                }
                return path.Replace('\\', '/');
            }
            return path;
        }

        public static string MakeAbsolutePathFromRRelative(this string rPath, string basePath) {
            if (string.IsNullOrEmpty(rPath)) {
                return basePath.Replace('/', '\\');
            }
            if (!string.IsNullOrEmpty(basePath)) {
                if(rPath.Length == 1 && rPath[0] == '~') {
                    return basePath;
                }
                else if(rPath.StartsWithOrdinal("~/")) {
                    rPath = rPath.Replace('/', '\\');
                    return Path.Combine(basePath, rPath.Substring(2));
                }
            }
            return rPath.Replace('/', '\\');
        }
    }
}
