// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.R.Support.Help.Definitions {
    public interface IFunctionIndex {
        Task BuildIndexAsync();
        void BuildIndexForPackage(IPackageInfo package);
        IFunctionInfo GetFunctionInfo(string functionName, Action<object> infoReadyCallback = null, object parameter = null);
        IReadOnlyCollection<INamedItemInfo> GetPackageFunctions(string packageName);
        void Initialize();
        void Terminate();
    }
}