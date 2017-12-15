// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.Setup.Configuration;

namespace Microsoft.UnitTests.Core {
    public sealed partial class VsPaths: Paths {
        private static Lazy<string> VsRootLazy { get; } = Lazy.Create(GetVsRoot);
        private static Lazy<string> VsCommonExtensionsLazy { get; } = Lazy.Create(() => Path.Combine(VsRoot, @"CommonExtensions\"));
        private static Lazy<string> VsPrivateAssembliesLazy { get; } = Lazy.Create(() => Path.Combine(VsRoot, @"PrivateAssemblies\"));
        private static Lazy<string> VsPublicAssembliesLazy { get; } = Lazy.Create(() => Path.Combine(VsRoot, @"PublicAssemblies\"));

        public static string VsRoot => VsRootLazy.Value;
        public static string VsCommonExtensions => VsCommonExtensionsLazy.Value;
        public static string VsPrivateAssemblies => VsPrivateAssembliesLazy.Value;
        public static string VsPublicAssemblies => VsPublicAssembliesLazy.Value;

        private static string GetVsRoot() {
            var configuration = (ISetupConfiguration2)new SetupConfiguration();
            var current = (ISetupInstance2)configuration.GetInstanceForCurrentProcess();
            var path = current.ResolvePath(current.GetProductPath());
            return Path.GetDirectoryName(path);

        }
    }
}
