// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Signatures;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Test.Utility {
    [ExcludeFromCodeCoverage]
    internal static class SignatureHelpSourceUtility {
        internal static Task AugmentSignatureHelpSessionAsync(this RSignatureHelpSource signatureHelpSource, ISignatureHelpSession session, IList<ISignature> signatures, AstRoot ast) {
            var tcs = new TaskCompletionSource<object>();

            var ready = signatureHelpSource.AugmentSignatureHelpSession(session, signatures, ast, (tv, sigs) => {
                foreach (var s in RSignatureHelpSource.ToVsEditorSignatures(sigs)) {
                    signatures.Add(s);
                }
                tcs.TrySetResult(null);
            });

            if (ready) {
                tcs.TrySetResult(null);
            }

            return tcs.Task;
        }
    }
}