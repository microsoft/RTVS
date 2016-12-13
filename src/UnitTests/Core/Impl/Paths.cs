// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.Win32;
using static System.FormattableString;

namespace Microsoft.UnitTests.Core {
    public static class Paths {
        private static string _vsRoot;
        private static string VsRoot {
            get {
                if (_vsRoot == null) {
#if VS14
                    _vsRoot = (string)Registry.GetValue(Invariant($"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\VisualStudio\\{Toolset.Version}"), "InstallDir", string.Empty);
#else
                    var buffer = new StringBuilder(512);
                    string ideFolder = @"Common7\IDE";
                    NativeMethods.GetModuleFileName(IntPtr.Zero, buffer, buffer.Capacity);

                    var testRunnerFolder = buffer.ToString();
                    var index = testRunnerFolder.IndexOfIgnoreCase(ideFolder);
                    if (index < 0) {
                        throw new InvalidOperationException("Unable to find VS IDE folder");
                    }

                    _vsRoot = testRunnerFolder.Substring(0, index + ideFolder.Length);
                }
#endif
                return _vsRoot;
            }
        }

        private static Lazy<string> VsPrivateAssembliesLazy { get; } = Lazy.Create(() => Path.Combine(VsRoot, @"PrivateAssemblies\"));
        private static Lazy<string> VsCommonExtensionsLazy { get; } = Lazy.Create(() => Path.Combine(VsRoot, @"CommonExtensions\"));
        private static Lazy<string> BinLazy { get; } = Lazy.Create(() => Path.GetDirectoryName(typeof(Paths).Assembly.GetAssemblyPath()));


        public static string VsPrivateAssemblies => VsPrivateAssembliesLazy.Value;
        public static string VsCommonExtensions => VsCommonExtensionsLazy.Value;
        public static string Bin => BinLazy.Value;

    }
}
