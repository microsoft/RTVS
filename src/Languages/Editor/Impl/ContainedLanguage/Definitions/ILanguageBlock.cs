// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using Microsoft.Languages.Core.Text;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    /// <summary>
    /// Language block that describes span in the primary language file 
    /// that belongs to primary or to one of the secondary language.
    /// Example: script or style block content in HTML file, R code in 
    /// R Markdown file.
    /// </summary>
    public interface ILanguageBlock : ITextRange {
        // Language command target
        ICommandTarget GetCommandTarget(ITextView textView);
        IContentType ContentType { get; }
    }
}
