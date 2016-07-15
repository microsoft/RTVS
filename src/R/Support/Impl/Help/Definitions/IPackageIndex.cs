// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Support.Help {
    public interface IPackageIndex {
        Task BuildIndexAsync(IFunctionIndex functionIndex, IRSession session);
        IEnumerable<IPackageInfo> Packages { get; }
        Task<IPackageInfo> GetPackageByNameAsync(string packageName);
    }
}