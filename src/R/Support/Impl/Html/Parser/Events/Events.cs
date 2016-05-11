// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser {
    /// <summary>
    /// Event argument that specifies text range
    /// </summary>
    public class HtmlParserRangeEventArgs : EventArgs {
        /// <summary>
        /// Item text range
        /// </summary>
        public ITextRange Range { get; private set; }

        public HtmlParserRangeEventArgs(ITextRange range) {
            Range = range;
        }
    }

    /// <summary>
    /// Event argument that specifies external (script or style) block range
    /// as well as any inner artifacts that appear inside the block.
    /// </summary>
    public class HtmlParserBlockRangeEventArgs : EventArgs {
        /// <summary>
        /// Block text range
        /// </summary>
        public ITextRange Range { get; private set; }
        public string ScriptType { get; private set; }

        public HtmlParserBlockRangeEventArgs(ITextRange range, string scriptType) {
            Range = range;
            ScriptType = scriptType;
        }
    }

    /// <summary>
    /// An argument for 'tag open' event
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class HtmlParserOpenTagEventArgs : EventArgs {
        /// <summary>
        /// Tag name token
        /// </summary>
        public NameToken NameToken { get; private set; }
        /// <summary>
        /// Position of a tag opening sequence (&lt; or &lt;/)
        /// </summary>
        public int OpenAngleBracketPosition { get; private set; }
        /// <summary>
        /// True if tag is a &lt;!doctype element
        /// </summary>
        public bool IsDocType { get; private set; }
        /// <summary>
        /// True if tag is an XML processing instruction
        /// </summary>
        public bool IsXmlPi { get; private set; }

        public HtmlParserOpenTagEventArgs(int openAngleBracketPosition, NameToken nameToken)
            : this(openAngleBracketPosition, nameToken, false, false) {
        }

        public HtmlParserOpenTagEventArgs(int openAngleBracketPosition, NameToken nameToken, bool isDocType, bool isXmlPi) {
            NameToken = nameToken;
            OpenAngleBracketPosition = openAngleBracketPosition;
            IsDocType = isDocType;
            IsXmlPi = isXmlPi;
        }
    }

    /// <summary>
    /// An argument for 'tag close' event
    /// </summary>
    public class HtmlParserCloseTagEventArgs : EventArgs {
        /// <summary>
        /// Range of the tag closing sequence: >, /> or nothing
        /// when tag is closed by the next element (in which case 
        /// range length is 0.
        /// </summary>
        public ITextRange CloseAngleBracket { get; private set; }
        /// <summary>
        /// True if tag is closed, i.e. > or /> is present
        /// </summary>
        public bool IsClosed { get; private set; }
        /// <summary>
        /// True if tag is short hand, i.e. closed by />
        /// </summary>
        public bool IsShorthand { get; private set; }

        public HtmlParserCloseTagEventArgs(ITextRange closeAngleBracketPosition, bool closed, bool shorthand) {
            CloseAngleBracket = closeAngleBracketPosition;
            IsClosed = closed;
            IsShorthand = shorthand;
        }
    }

    /// <summary>
    /// An argument for 'on attribute' event
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class HtmlParserAttributeEventArgs : EventArgs {
        /// <summary>
        /// Attribute token
        /// </summary>
        public AttributeToken AttributeToken { get; private set; }

        public HtmlParserAttributeEventArgs(AttributeToken attributeToken) {
            AttributeToken = attributeToken;
        }
    }

    /// <summary>
    /// An argument for 'on comment' event
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class HtmlParserCommentEventArgs : EventArgs {
        /// <summary>
        /// Comment token
        /// </summary>
        public CommentToken CommentToken { get; private set; }

        public HtmlParserCommentEventArgs(CommentToken commentToken) {
            CommentToken = commentToken;
        }
    }
}
