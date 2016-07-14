// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.Win32;
using static System.FormattableString;

namespace Microsoft.UnitTests.Core {
    public static class Paths {
        private static Lazy<string> VsRootLazy { get; } = Lazy.Create(() => (string)Registry.GetValue(Invariant($"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\VisualStudio\\{Toolset.Version}"), "InstallDir", string.Empty));
        private static Lazy<string> VsPrivateAssembliesLazy { get; } = Lazy.Create(() => Path.Combine(VsRootLazy.Value, @"PrivateAssemblies\"));
        private static Lazy<string> VsCommonExtensionsLazy { get; } = Lazy.Create(() => Path.Combine(VsRootLazy.Value, @"CommonExtensions\"));
        private static Lazy<string> BinLazy { get; } = Lazy.Create(() => Path.GetDirectoryName(typeof(Paths).Assembly.GetAssemblyPath()));


        public static string VsRoot => VsRootLazy.Value;
        public static string VsPrivateAssemblies => VsPrivateAssembliesLazy.Value;
        public static string VsCommonExtensions => VsCommonExtensionsLazy.Value;
        public static string Bin => BinLazy.Value;
        
    }
}
