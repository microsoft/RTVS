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

            ITrackingSpan applicableSpan;
            var ready = quickInfoSource.AugmentQuickInfoSession(ast, textBuffer, position, quickInfoSession, quickInfoContent, out applicableSpan, (o, p) => {
                ITrackingSpan result;
                quickInfoSource.AugmentQuickInfoSession(ast, textBuffer, position, quickInfoSession, quickInfoContent, out result, null, p);
                tcs.TrySetResult(result);
            }, null);

            if (ready) {
                tcs.TrySetResult(applicableSpan);
            }

            return tcs.Task;
        }
    }
}
