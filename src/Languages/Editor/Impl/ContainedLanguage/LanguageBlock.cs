// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    /// <summary>
    /// Language block that describes span in an HTML file that belongs to primary or to 
    /// one of the secondary languages such as to CSS, script, server code or HTML markup. 
    /// </summary>
    public abstract class LanguageBlock : TextRange, IComparable {
        public abstract ICommandTarget GetCommandTarget(ITextView textView);

        #region Constructors
        public LanguageBlock(int start, int length) :
            base(start, length) {
        }
        #endregion

        #region 
        public int CompareTo(object obj) {
            ITextRange other = (ITextRange)obj;
            if (End <= other.End) {
                return -1;
            }
            if (AreEqual(this, other)) {
                return 0;
            }
            return 1;
        }
        #endregion
    }
}
