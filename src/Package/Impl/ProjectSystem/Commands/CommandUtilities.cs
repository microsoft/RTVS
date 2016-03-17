// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.ProjectSystem.Designers;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    internal static class CommandUtilities {
        public static string GetSingleNodePath(this IImmutableSet<IProjectTree> nodes) {
            if (nodes != null && nodes.Count == 1) {
                return nodes.FirstOrDefault().FilePath;
            }
            return string.Empty;
        }

        public static bool IsSingleNodePath(this IImmutableSet<IProjectTree> nodes) {
            if (nodes != null && nodes.Count == 1) {
                return !string.IsNullOrEmpty(nodes.First().FilePath);
            }
            return false;
        }

        public static bool IsFolder(this IImmutableSet<IProjectTree> nodes) {
            if (nodes != null && nodes.Count == 1) {
                return nodes.First().IsFolder;
            }
            return false;
        }

        public static string GetNodeFolderPath(this IImmutableSet<IProjectTree> nodes) {
            var path = nodes.GetSingleNodePath();
            if (!string.IsNullOrEmpty(path)) {
                if (Directory.Exists(path)) {
                    return path;
                } else if (File.Exists(path)) {
                    return Path.GetDirectoryName(path);
                }
            }
            return string.Empty;
        }

        public static IEnumerable<string> GetSelectedNodesPaths(this IImmutableSet<IProjectTree> nodes) {
            if (nodes != null && nodes.Count > 0) {
                return nodes.Where(x => (x != null && !string.IsNullOrEmpty(x.FilePath))).Select(x => x.FilePath);
            }
            return Enumerable.Empty<string>();
        }
    }
}
