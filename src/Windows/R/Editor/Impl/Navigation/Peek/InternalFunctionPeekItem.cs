// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Navigation.Peek {
    /// <summary>
    /// Peekable item (Peek Definition) representing non-user R function
    /// </summary>
    internal sealed class InternalFunctionPeekItem : PeekItemBase {
        private readonly InternalFunctionPeekResultSource _source;

        public InternalFunctionPeekItem(string sourceFileName, Span sourceSpan, string functionName, IPeekResultFactory peekResultFactory, ICoreShell shell) :
            base(functionName, peekResultFactory, shell) {
            // Create source right away so it can start asynchronous function fetching
            // so by the time GetOrCreateResultSource is called the task may be already underway.
            _source = new InternalFunctionPeekResultSource(sourceFileName, sourceSpan, functionName, this, shell);
        }

        public override IPeekResultSource GetOrCreateResultSource(string relationshipName) {
            return _source;
        }
    }
}
