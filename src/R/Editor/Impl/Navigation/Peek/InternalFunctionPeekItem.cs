// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Navigation.Peek {
    internal sealed class InternalFunctionPeekItem : PeekItemBase {
        private readonly InternalFunctionPeekResultSource _source;

        public InternalFunctionPeekItem(string functionName, IPeekResultFactory peekResultFactory) :
            base(functionName, peekResultFactory) {
            _source = new InternalFunctionPeekResultSource(this, DisplayName);
        }

        public override IPeekResultSource GetOrCreateResultSource(string relationshipName) {
            return _source;
        }
    }
}
