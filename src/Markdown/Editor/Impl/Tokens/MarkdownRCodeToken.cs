// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Languages.Core.Classification;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Tokens;

namespace Microsoft.Markdown.Editor.Tokens {
    /// <summary>
    /// Composite token that represents R code inside a markdown document
    /// </summary>
    [DebuggerDisplay("[{TokenType} : {Start}...{End}), Length = {Length}")]
    public class MarkdownRCodeToken : MarkdownToken, ICompositeToken {
        private ITextProvider _textProvider;

        public MarkdownRCodeToken(int start, int length, ITextProvider textProvider) :
            base(MarkdownTokenType.CodeContent, new TextRange(start, length)) {
            _textProvider = textProvider;
        }

        public ReadOnlyCollection<object> TokenList {
            get {
                var rTokenizer = new RTokenizer();
                var tokens = rTokenizer.Tokenize(_textProvider, Start, Length);
                var list = new List<object>(tokens);
                return new ReadOnlyCollection<object>(list);
            }
        }

        public string ContentType {
            get { return "R"; }
        }
    }
}
