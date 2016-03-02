// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;

namespace Microsoft.Common.Core.Test.Utility {
    public static class RUtilities {
        public static string FindExistingRBasePath() {
            // Test settings are fixed and are unrelated to what is stored in VS.
            // Therefore we need to look up R when it is not in the registry.
            string programFiles = Environment.GetEnvironmentVariable("ProgramW6432");
            if (programFiles == null) {
                return string.Empty;
            }

            var topDir = new DirectoryInfo(Path.Combine(programFiles, "R"));
            if (!topDir.Exists) {
                topDir = new DirectoryInfo(Path.Combine(programFiles, @"Microsoft\MRO-for-RRE\8.0"));
                if (!topDir.Exists) {
                    return string.Empty;
                }
            }

            foreach (var dir in topDir.EnumerateDirectories()) {
                if (dir.Name.StartsWith("R-3.2.")) {
                    return dir.FullName;
                }
            }

            return string.Empty;
        }
    }
}
