// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Signatures;

namespace Microsoft.R.Editor.Signatures {
    public interface IFunctionSignatureSource {
        Task<IEnumerable<IFunctionSignature>> GetSignaturesAsync(IRCompletionContext context);
    }
}
