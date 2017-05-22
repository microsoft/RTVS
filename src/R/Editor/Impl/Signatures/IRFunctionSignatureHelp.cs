// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Signatures;
using Microsoft.R.Editor.Functions;

namespace Microsoft.R.Editor.Signatures
{
    public interface IRFunctionSignatureHelp: IFunctionSignatureHelp {
        ISignatureInfo SignatureInfo { get; }
        IEditorIntellisenseSession Session { get; set; }
    }
}
