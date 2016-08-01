// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    internal static class ProjectSettingsFiles {
        public const string SettingsFilePattern = "*settings*.r";

        public static bool IsProjectSettingFile(string fileName) {
            return fileName.StartsWithIgnoreCase("settings") || fileName.EndsWithIgnoreCase(".settings.r");
        }
    }
}
