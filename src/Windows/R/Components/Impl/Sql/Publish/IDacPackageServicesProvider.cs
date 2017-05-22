// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.Sql.Publish {
    public interface IDacPackageServicesProvider {
        IDacPackageServices GetDacPackageServices(bool showMessage = false);
    }
}
