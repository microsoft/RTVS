// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;

namespace Microsoft.VisualStudio.R.Package.Shell {
    /// <summary>
    /// Application shell provides access to services
    /// such as composition container, export provider, global VS IDE
    /// services and so on.
    /// </summary>
    [Export(typeof(ICoreShell))]
    public sealed partial class VsAppShell {
        private static void SetupTestInstance() {
            var thisAssembly = Assembly.GetExecutingAssembly().GetAssemblyPath();
            var assemblyLoc = Path.GetDirectoryName(thisAssembly);
            var packageTestAssemblyPath = Path.Combine(assemblyLoc, "Microsoft.VisualStudio.R.Package.Test.dll");
            Assembly testAssembly = null;

            // Catch exception when loading assembly since it is missing in non-test
            // environment but do throw when it is present but test types cannot be created.
            try {
                testAssembly = Assembly.LoadFrom(packageTestAssemblyPath);
            } catch (Exception) { }

            if (testAssembly != null) {
                var types = testAssembly.GetTypes();
                var classes = types.Where(x => x.IsClass);

                var testshell = classes.FirstOrDefault(c => c.Name.Contains("VsAppShellTestSetup"));
                Debug.Assert(testshell != null);

                var mi = testshell.GetMethod("Setup", BindingFlags.Static | BindingFlags.Public);
                mi.Invoke(null, new object[] { _instance });
            } else {
                Debug.Fail("Unable to create VsAppShellTestSetup in Microsoft.VisualStudio.R.Package.Test.dll");
            }
        }
    }
}
