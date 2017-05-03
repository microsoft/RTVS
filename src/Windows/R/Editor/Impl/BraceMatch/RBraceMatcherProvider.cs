// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.BraceMatch.Definitions;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.BraceMatch {
    [Export(typeof(IBraceMatcherProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal class RBraceMatcherProvider : IBraceMatcherProvider {
        public IBraceMatcher CreateBraceMatcher(ITextView textView, ITextBuffer textBuffer)
            => textBuffer.Properties.GetOrCreateSingletonProperty(() => new RBraceMatcher(textView, textBuffer));
    }
}
