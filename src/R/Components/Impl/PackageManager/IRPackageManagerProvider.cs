// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;

namespace Microsoft.R.Components.PackageManager {
    public interface IRPackageManagerProvider {
        IRPackageManager CreateRPackageManager(IRSettings settings, IRInteractiveWorkflow interactiveWorkflow);
    }
}