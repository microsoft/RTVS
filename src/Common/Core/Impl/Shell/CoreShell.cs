using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Common.Core.Shell {
    public static class CoreShell {
        // Normally shell object is set by the package or other top-level
        // application object that implements services needed by various 
        // modules such as MEF composition container and so on. However, 
        // in tests the application is not and objectsoften are instantiated
        // in isolation. In this case code uses reflection to instatiate 
        // service provider with a specific name.
        public static void TryCreateTestInstance(string assemblyName, string className) {
            string thisAssembly = Assembly.GetExecutingAssembly().Location;
            string assemblyLoc = Path.GetDirectoryName(thisAssembly);
            string packageTestAssemblyPath = Path.Combine(assemblyLoc, assemblyName);

            Assembly testAssembly = Assembly.LoadFrom(packageTestAssemblyPath);
            if (testAssembly != null) {
                Type[] types = testAssembly.GetTypes();
                IEnumerable<Type> classes = types.Where(x => x.IsClass);

                Type testAppShell = classes.FirstOrDefault(c => c.Name.Contains(className));
                Debug.Assert(testAppShell != null);

                MethodInfo mi = testAppShell.GetMethod("Create", BindingFlags.Static | BindingFlags.Public);
                mi.Invoke(null, null);
            }
        }
    }
}
