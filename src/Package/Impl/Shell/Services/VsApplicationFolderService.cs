// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using Microsoft.Common.Core;
using Microsoft.Common.Core.OS;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal sealed class VsApplicationFolderService : IApplicationFolderService {
        public string ApplicationDataFolder => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public string ApplicationFolder {
            get {
                var asmPath = Assembly.GetExecutingAssembly().GetAssemblyPath();
                return Path.GetDirectoryName(asmPath);
            }
        }
    }
}
