// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;

namespace Microsoft.VisualStudio.R.Package.Sql {
    internal static class SProcFileExtensions {
        public const string QueryFileExtension = ".Query.sql";
        public const string SProcFileExtension = ".Template.sql";

        public static string ToQueryFilePath(this string rFilePath) {
            return !string.IsNullOrEmpty(rFilePath)
                       ? ChangeExtension(rFilePath, QueryFileExtension)
                       : rFilePath;
        }

        public static string ToSProcFilePath(this string rFilePath) {
            return !string.IsNullOrEmpty(rFilePath)
                    ? ChangeExtension(rFilePath, SProcFileExtension)
                    : rFilePath;
        }

        private static string ChangeExtension(string filePath, string extension) {
            var name = Path.GetFileNameWithoutExtension(filePath);
            return !string.IsNullOrEmpty(name)
                    ? Path.Combine(Path.GetDirectoryName(filePath), name + extension)
                    : string.Empty;
        }
    }
}
