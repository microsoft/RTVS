// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.R.Support.Help {
    public interface IFunctionIndex {
        Task BuildIndexAsync(IPackageIndex packageIndex = null);
        IFunctionInfo GetFunctionInfo(string functionName, Action<object> infoReadyCallback = null, object parameter = null);
    }
}