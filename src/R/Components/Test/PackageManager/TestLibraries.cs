// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Test.PackageManager {
    [ExcludeFromCodeCoverage]
    internal static class TestLibraries {
        public static Task SetLocalLibsAsync(IRExpressionEvaluator eval, params string[] libPaths) {
            foreach (var libPath in libPaths.Where(libPath => !Directory.Exists(libPath))) {
                Directory.CreateDirectory(libPath);
            }

            var paths = string.Join(",", libPaths.Select(p => p.ToRPath().ToRStringLiteral()));
            var code = $".libPaths(c({paths}))";
            return eval.ExecuteAsync(code);
        }

        public static Task SetLocalLibraryAsync(IRExpressionEvaluator eval, MethodInfo testMethod, TestFilesFixture testFiles) {
            return SetLocalLibsAsync(eval, Path.Combine(testFiles.LibraryDestinationPath, testMethod.Name));
        }
    }
}