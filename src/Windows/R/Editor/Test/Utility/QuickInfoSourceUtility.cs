// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.QuickInfo;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Utility {
    [ExcludeFromCodeCoverage]
    internal static class QuickInfoSourceUtility {
        internal static Task<ITrackingSpan> AugmentQuickInfoSessionAsync(
            this QuickInfoSource quickInfoSource
            , AstRoot ast
            , ITextBuffer textBuffer
            , int position
            , IQuickInfoSession quickInfoSession
            , IList<object> quickInfoContent) {

            var tcs = new TaskCompletionSource<ITrackingSpan>();

            var ready = quickInfoSource.AugmentQuickInfoSession(ast, textBuffer, position, quickInfoSession, quickInfoContent, out ITrackingSpan applicableSpan, (infos, o) => {
                QuickInfoSource.GetCachedSignatures(quickInfoContent, textBuffer, position, infos, out ITrackingSpan result);
                tcs.TrySetResult(result);
            });

            if (ready) {
                tcs.TrySetResult(applicableSpan);
            }

            return tcs.Task;
        }
    }
}
