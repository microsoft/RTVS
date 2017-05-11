// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Completions;

namespace Microsoft.R.Editor.Signatures {
    public interface IRFunctionSignatureEngine {
        Task<IEnumerable<IRFunctionSignatureHelp>> GetSignaturesAsync(IRIntellisenseContext context);
    }
}
