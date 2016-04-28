// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    [Flags]
    public enum ViewerCapabilities {
        Function = 1,
        List = 2,
        Table = 4,
    }
}
