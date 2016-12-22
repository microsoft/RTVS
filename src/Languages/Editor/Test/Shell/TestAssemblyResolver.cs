// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using Microsoft.UnitTests.Core;

namespace Microsoft.Languages.Editor.Test.Shell {
    /// <summary>
    /// Locates and resolves Visual Studio assemblies that test may need.
    /// The class stays connected to the appdomain events until test run finishes.
    /// </summary>
    public sealed class TestAssemblyResolver {
        public TestAssemblyResolver() {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            int index = args.Name.IndexOf(',');
            string name = index >= 0 ? args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll" : args.Name;
            Assembly asm = null;

            string path = Path.Combine(AssemblyLocations.PrivatePath, name);
            if (File.Exists(path)) {
                asm = Assembly.LoadFrom(path);
            }

            if (asm == null) {
                path = Path.Combine(Paths.VsRoot, name);
                if (File.Exists(path)) {
                    asm = Assembly.LoadFrom(path);
                }
            }

            if (asm == null) {
                path = Path.Combine(AssemblyLocations.SharedPath, name);
                if (File.Exists(path)) {
                    asm = Assembly.LoadFrom(path);
                }
            }

            return asm;
        }
    }
}
