// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    internal interface IConfiguredRProjectExportProvider {
        T GetExport<T>(UnconfiguredProject unconfigProject, string configurationName);
        T GetExport<T>(IVsHierarchy projectHierarchy, string configurationName);
    }
}
