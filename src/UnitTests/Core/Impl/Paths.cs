// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.Common.Core;

namespace Microsoft.UnitTests.Core {
    public class Paths {
        private static Lazy<string> BinLazy { get; } = Lazy.Create(() => Path.GetDirectoryName(typeof(Paths).Assembly.GetAssemblyPath()));
        public static string Bin => BinLazy.Value;
    }
}
