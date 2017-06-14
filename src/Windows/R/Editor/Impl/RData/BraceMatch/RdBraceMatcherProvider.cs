// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.BraceMatch.Definitions;
using Microsoft.R.Editor.RData.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.RData.BraceMatch {
    [Export(typeof(IBraceMatcherProvider))]
    [ContentType(RdContentTypeDefinition.ContentType)]
    internal class MdBraceMatcherProvider : IBraceMatcherProvider {
        public IBraceMatcher CreateBraceMatcher(ITextView textView, ITextBuffer textBuffer) 
            => new RdBraceMatcher(textView, textBuffer);
    }
}
