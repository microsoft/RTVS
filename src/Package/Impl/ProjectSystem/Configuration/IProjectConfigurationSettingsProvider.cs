// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration {
    /// <summary>
    /// Provides access to the current project configuration settings collection.
    /// The collection is shared and there may be multiple concurrent users 
    /// such as when project property page is open and user also invokes 
    /// 'Add Database Connection' command. Dispose object to release access
    /// to the collection. All access is read/write. Do not cache the object.
    /// </summary>
    public interface IProjectConfigurationSettingsProvider {
        Task<IProjectConfigurationSettingsAccess> OpenProjectSettingsAccessAsync(UnconfiguredProject project, IRProjectProperties propertes);
        Task<IProjectConfigurationSettingsAccess> OpenProjectSettingsAccessAsync(ConfiguredProject project);
    }
}
