// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Host.Client;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    public static class IRProjectPropertiesExtensions {
        public static async Task<string> ToRRemotePathAsync(this IRProjectProperties properties, string projectRelativeFilePath) {
            string remotePath = await properties.GetRemoteProjectPathAsync();
            string projectName = properties.GetProjectName();
            return (remotePath + projectName + "/" + projectRelativeFilePath).ToRPath();
        }
    }
}
