// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.R.Editor.Signatures.Definitions;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;

namespace Microsoft.R.Editor.Signatures.Providers {
    [Export(typeof(IFunctionInformationProvider))]
    internal sealed class UserFunctionsInformationProvider : IFunctionInformationProvider {
        public IFunctionInfo GetFunctionInfo(string functionName,
                                  Action<object> infoReadyCallback = null, object parameter = null) {
            // Get collection of function signatures from documentation (parsed RD file)
            return FunctionIndex.GetFunctionInfo(functionName, infoReadyCallback, parameter);
        }
    }
}
