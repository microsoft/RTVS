// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.R.Components.PackageManager.Model;

namespace Microsoft.R.Support.Help.Packages {
    /// <summary>
    /// Represents R package installed on user machine
    /// </summary>
    internal sealed class PackageInfo : NamedItemInfo, IPackageInfo {
        private readonly IFunctionIndex _functionIndex;

        public PackageInfo(IFunctionIndex functionIndex, string name, string description) :
            base(name, description, NamedItemType.Package) {
            _functionIndex = functionIndex;
        }

        #region IPackageInfo
        /// <summary>
        /// Collection of functions in the package
        /// </summary>
        public IEnumerable<INamedItemInfo> Functions => _functionIndex.GetPackageFunctions(this.Name);
        #endregion

    }
}
