// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Microsoft.Common.Core.IO {
    public sealed class UnixFileSystem : FileSystem {
        public override string GetDownloadsPath(string fileName) {
            return Path.Combine("~/Downloads", fileName);
        }
    }
}
