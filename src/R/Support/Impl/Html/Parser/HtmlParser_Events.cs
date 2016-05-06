// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Html.Core.Parser {
    public sealed partial class HtmlParser {
        /// <summary>
        /// Fires when parsing session begins
        /// </summary>
        public event EventHandler<HtmlParserRangeEventArgs> ParsingStarting;
        /// <summary>
        /// Fires when parser session ends
        /// </summary>
        public event EventHandler<HtmlParserRangeEventArgs> ParsingComplete;

        /// <summary>
        /// Fires when parser finds an opening tag
        /// </summary>
        public event EventHandler<HtmlParserOpenTagEventArgs> StartTagOpen;
        /// <summary>
        /// Fires when parser closes an open start tag. Tag may be 
        /// closed by >, />, by start of the next element (&lt;) or by
        /// end of the stream.
        /// </summary>
        public event EventHandler<HtmlParserCloseTagEventArgs> StartTagClose;
        /// <summary>
        /// Fires when parser finds a closing tag
        /// </summary>
        public event EventHandler<HtmlParserOpenTagEventArgs> EndTagOpen;
        /// <summary>
        /// Fires when parser closes an open end tag. Tag may be 
        /// closed by >, />, by start of the next element (&lt;) or by
        /// end of the stream.
        /// </summary>
        public event EventHandler<HtmlParserCloseTagEventArgs> EndTagClose;
        /// <summary>
        /// Fires when parser finished processing of an attribute
        /// </summary>
        public event EventHandler<HtmlParserAttributeEventArgs> AttributeFound;
        /// <summary>
        /// Fires when parser finds a comment
        /// </summary>
        public event EventHandler<HtmlParserCommentEventArgs> CommentFound;
        /// <summary>
        /// Fires when parser finds a script block
        /// </summary>
        public event EventHandler<HtmlParserBlockRangeEventArgs> ScriptBlockFound;
        /// <summary>
        /// Fires when parser finds a style block
        /// </summary>
        public event EventHandler<HtmlParserBlockRangeEventArgs> StyleBlockFound;
        /// <summary>
        /// Fires when parser finds an HTML entity
        /// </summary>
        public event EventHandler<HtmlParserRangeEventArgs> EntityFound;
    }
}
