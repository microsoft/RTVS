// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Components.PackageManager {
    public interface IPackageInstallationNotifications {
        Task BeforePackagesInstalledAsync(CancellationToken cancellationToken);
        Task AfterPackagesInstalledAsync(CancellationToken cancellationToken);
    }
}
