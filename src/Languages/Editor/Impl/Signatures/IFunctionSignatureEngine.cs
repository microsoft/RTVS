// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Completions;

namespace Microsoft.Languages.Editor.Signatures {
    public interface IFunctionSignatureEngine {
        Task<IEnumerable<IFunctionSignatureHelp>> GetSignaturesAsync(IIntellisenseContext context);
    }
}
