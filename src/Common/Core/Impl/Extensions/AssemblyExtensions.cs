using System;
using System.Reflection;

namespace Microsoft.Common.Core {
    public static class AssemblyExtensions {
        public static string GetAssemblyPath(this Assembly assembly) {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            return new Uri(codeBase).LocalPath;
        }
    }
}