// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Tasks;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Test {
    internal static class EventTaskSources {
        public static class MsBuildFileSystemWatcher {
            public static readonly EventTaskSource<FileSystemMirroring.IO.MsBuildFileSystemWatcher, EventArgs> Error =
                new EventTaskSource<FileSystemMirroring.IO.MsBuildFileSystemWatcher, EventArgs>((o, e) => o.Error += e, (o, e) => o.Error -= e);
        }
    }
}
