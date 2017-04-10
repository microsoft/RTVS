// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Editor.Functions {
    /// <summary>
    /// Type of the item (function, constant)
    /// </summary>
    public enum NamedItemType {
        None,
        Package,
        Function,
        Constant,
        Parameter,
        Variable,
        Dataset
    }
}
