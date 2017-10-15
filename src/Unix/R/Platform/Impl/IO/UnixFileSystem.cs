// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using Microsoft.Common.Core.IO;

namespace Microsoft.R.Platform.IO {
    public sealed class UnixFileSystem : FileSystem {
        public override string GetDownloadsPath(string fileName) => Path.Combine("~/Downloads", fileName);
    }
}
