// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Text;
using Microsoft.Common.Core;

namespace Microsoft.UnitTests.Core {
    public static class Paths {
        private static object _lock = new object();
        private static string _vsRoot;
        public static string VsRoot {
            get {
                lock (_lock) {
                    if (_vsRoot == null) {
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
                    return _vsRoot;
                }
            }
        }

        private static Lazy<string> VsCommonExtensionsLazy { get; } = Lazy.Create(() => Path.Combine(VsRoot, @"CommonExtensions\"));
        private static Lazy<string> VsPrivateAssembliesLazy { get; } = Lazy.Create(() => Path.Combine(VsRoot, @"PrivateAssemblies\"));
        private static Lazy<string> VsPublicAssembliesLazy { get; } = Lazy.Create(() => Path.Combine(VsRoot, @"PublicAssemblies\"));
        private static Lazy<string> BinLazy { get; } = Lazy.Create(() => Path.GetDirectoryName(typeof(Paths).Assembly.GetAssemblyPath()));

        public static string VsCommonExtensions => VsCommonExtensionsLazy.Value;
        public static string VsPrivateAssemblies => VsPrivateAssembliesLazy.Value;
        public static string VsPublicAssemblies => VsPublicAssembliesLazy.Value;
        public static string Bin => BinLazy.Value;

    }
}
