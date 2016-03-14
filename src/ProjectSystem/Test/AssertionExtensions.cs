// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Test.Assertions;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Test {
    [ExcludeFromCodeCoverage]
    internal static class AssertionExtensions {
        public static MsBuildFileSystemWatcherChangesetAssertions Should(this MsBuildFileSystemWatcher.Changeset token) {
            return new MsBuildFileSystemWatcherChangesetAssertions(token);
        }
    }
}