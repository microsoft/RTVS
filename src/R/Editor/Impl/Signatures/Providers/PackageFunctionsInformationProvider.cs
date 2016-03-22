// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Editor.Signatures.Definitions;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;

namespace Microsoft.R.Editor.Signatures.Providers {
    internal sealed class PackageFunctionsInformationProvider : IFunctionInformationProvider {
        public IFunctionInfo GetFunctionInfo(RSignatureHelpContext context,     string functionName,
                                  Action<object> infoReadyCallback = null, object parameter = null) {
            // Get collection of function signatures from documentation (parsed RD file)
            return FunctionIndex.GetFunctionInfo(functionName, infoReadyCallback, parameter);
        }
    }
}
