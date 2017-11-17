// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Common.Core;

namespace Microsoft.UnitTests.Core {
    public sealed class VsPaths: Paths {
        private static Lazy<string> VsRootLazy { get; } = Lazy.Create(GetVsRoot);
        private static Lazy<string> VsCommonExtensionsLazy { get; } = Lazy.Create(() => Path.Combine(VsRoot, @"CommonExtensions\"));
        private static Lazy<string> VsPrivateAssembliesLazy { get; } = Lazy.Create(() => Path.Combine(VsRoot, @"PrivateAssemblies\"));
        private static Lazy<string> VsPublicAssembliesLazy { get; } = Lazy.Create(() => Path.Combine(VsRoot, @"PublicAssemblies\"));

        public static string VsRoot => VsRootLazy.Value;
        public static string VsCommonExtensions => VsCommonExtensionsLazy.Value;
        public static string VsPrivateAssemblies => VsPrivateAssembliesLazy.Value;
        public static string VsPublicAssemblies => VsPublicAssembliesLazy.Value;

        private static string GetVsRoot() {
            // See https://github.com/Microsoft/vswhere
            var processPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft Visual Studio", "Installer", "vswhere.exe");
            var psi = new ProcessStartInfo {
                FileName = processPath,
                Arguments = "-latest -property productPath",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            var devenvPath = process.StandardOutput.ReadLine();
            return Path.GetDirectoryName(devenvPath);
        }
    }
}
