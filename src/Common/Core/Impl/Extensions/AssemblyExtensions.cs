using System;
using System.Reflection;

namespace Microsoft.Common.Core {
    public static class AssemblyExtensions {
        public static string GetAssemblyPath(this Assembly assembly) {
            var codeBase = assembly.CodeBase;
            return new Uri(codeBase).LocalPath;
        }
    }
}