// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Editor.QuickInfo;

namespace Microsoft.R.Editor.Signatures {
    public interface IRFunctionSignatureEngine {
        IEnumerable<IRFunctionSignatureHelp> GetSignaturesAsync(IRIntellisenseContext context , Action<IEnumerable<IRFunctionSignatureHelp>> callback);

        IEnumerable<IRFunctionQuickInfo> GetQuickInfosAsync(IRIntellisenseContext context, Action<IEnumerable<IRFunctionQuickInfo>> callback);
    }
}
