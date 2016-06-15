// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell.Interop;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    internal interface IConfiguredRProjectExportProvider {
        Task<T> GetExportAsync<T>(UnconfiguredProject unconfigProject, string configurationName);
        Task<T> GetExportAsync<T>(IVsHierarchy projectHierarchy, string configurationName);
    }
}
