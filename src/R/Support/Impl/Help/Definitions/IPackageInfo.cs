// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.R.Support.Help {
    public interface IPackageInfo : INamedItemInfo, IDisposable {
        /// <summary>
        /// List of functions in the package
        /// </summary>
        IEnumerable<INamedItemInfo> Functions { get; }
    }
}
