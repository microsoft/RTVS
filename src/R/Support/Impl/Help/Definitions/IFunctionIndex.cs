// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Support.Help {
    public interface IFunctionIndex: IDisposable {
        Task BuildIndexAsync();
        IFunctionInfo GetFunctionInfo(string functionName, Action<object> infoReadyCallback = null, object parameter = null);
        IReadOnlyCollection<INamedItemInfo> GetPackageFunctions(string packageName);
        bool IsReady { get; }
        IRSession RSession { get; }
    }
}