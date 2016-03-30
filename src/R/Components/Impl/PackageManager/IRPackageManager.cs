// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.R.Components.PackageManager.Model;

namespace Microsoft.R.Components.PackageManager {
    public interface IRPackageManager : IDisposable {
        IRPackageManagerVisualComponent VisualComponent { get; }

        IRPackageManagerVisualComponent GetOrCreateVisualComponent(IRPackageManagerVisualComponentContainerFactory visualComponentContainerFactory, int instanceId = 0);

        Task<IReadOnlyList<RPackage>> GetInstalledPackagesAsync();

        Task<IReadOnlyList<RPackage>> GetAvailablePackagesAsync();

        Task AddAdditionalPackageInfoAsync(RPackage pkg);

        Task<RPackage> GetAdditionalPackageInfoAsync(string pkg, string repository);
    }
}