// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using Microsoft.Common.Core;

namespace Microsoft.VisualStudio.R.Package.Packages {
    internal static class AssemblyResolver {
        private static bool _initialized;

        public static void Init() {
            if (!_initialized) {
                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
                _initialized = true;
            }
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args) {
            if (args.Name.StartsWithOrdinal("Microsoft.R.") || args.Name.StartsWithOrdinal("Microsoft.VisualStudio.R.")) {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetAssemblyPath());
                var asmName = args.Name.Substring(0, args.Name.IndexOf(','));
                return Assembly.LoadFrom(Path.Combine(path, $"{asmName}.dll"));
            }
            return null;
        }

        public static void Close() {
            if (_initialized) {
                AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
                _initialized = false;
            }
        }
    }
}
