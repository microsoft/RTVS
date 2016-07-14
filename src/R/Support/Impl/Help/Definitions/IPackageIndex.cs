// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.R.Support.Help {
    public interface IPackageIndex {
        Task BuildIndexAsync(IFunctionIndex functionIndex);
        IEnumerable<IPackageInfo> Packages { get; }
        IPackageInfo GetPackageByName(string packageName);
    }
}