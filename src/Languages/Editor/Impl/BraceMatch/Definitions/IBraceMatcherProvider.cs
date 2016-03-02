// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.BraceMatch.Definitions {
    /// <summary>
    /// Brace matcher factory typically exported via MEF using BraceMatcherProviderAttribute 
    /// attribute for a given content type.
    /// </summary>
    public interface IBraceMatcherProvider {
        /// <summary>
        /// Creates brace or element matcher for this content type.
        /// </summary>
        /// <param name="textView">Text view</param>
        /// <param name="textBuffer">Text buffer (secondary buffer in contained language scenarios)</param>
        /// <returns>Brace matcher object</returns>
        IBraceMatcher CreateBraceMatcher(ITextView textView, ITextBuffer textBuffer);
    }
}
