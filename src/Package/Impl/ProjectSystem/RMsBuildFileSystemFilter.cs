// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    internal sealed class RMsBuildFileSystemFilter : IMsBuildFileSystemFilter {
        public bool IsFileAllowed(string relativePath, FileAttributes attributes) {
            return !attributes.HasFlag(FileAttributes.Hidden)
                && !HasExtension(relativePath, ".user", RContentTypeDefinition.VsRProjectExtension, ".sln");
        }

        public bool IsDirectoryAllowed(string relativePath, FileAttributes attributes) {
            return !attributes.HasFlag(FileAttributes.Hidden);
        }

        public void Seal() { }

        private static bool HasExtension(string filePath, params string[] possibleExtensions) {
            var extension = Path.GetExtension(filePath);
            return !string.IsNullOrEmpty(extension) && possibleExtensions.Any(pe => extension.EqualsIgnoreCase(pe));
        }
    }
}