// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using Microsoft.Languages.Core.Text;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    /// <summary>
    /// Language block that describes span in an HTML file that belongs to primary or to 
    /// one of the secondary languages such as to CSS, script, server code or HTML markup. 
    /// </summary>
    public abstract class LanguageBlock : TextRange, ILanguageBlock {
        public abstract ICommandTarget GetCommandTarget(ITextView textView);
        public abstract IContentType ContentType { get; }

        #region Constructors
        public LanguageBlock(int start, int length) :
            base(start, length) {
        }
        #endregion
    }
}
