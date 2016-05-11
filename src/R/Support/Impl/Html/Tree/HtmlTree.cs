// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Html.Core.Parser;
using Microsoft.Html.Core.Tree.Builder;
using Microsoft.Html.Core.Tree.Nodes;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Tree {
    /// <summary>
    /// HTML Tree. 
    /// </summary>
    /// <remarks>
    /// The tree is static, it does not listen to OnTextChange.
    /// If you need dynamic tree, look at <seealso cref="HtmlEditorTree"/>
    /// in Microsoft.Html.Editor
    /// </remarks>
    public class HtmlTree : IHtmlTreeVisitorPattern {
        /// <summary>
        /// Tree root node. Root node does not represent any particular 
        /// element but it has to be there since many HTML documents 
        /// do not have a single root element. Tree is created by a
        /// <seealso cref="HtmlTreeBuilder"/> that listens to events
        /// from <seealso cref="HtmlParser"/>.
        /// </summary>
        public RootNode RootNode { get; internal set; }

        /// <summary>
        /// Text provider associated with this tree. Typically
        /// a snapshot of text that was used to build the tree.
        /// </summary>
        public ITextProvider Text { get; internal set; }

        /// <summary>
        /// &lt;!DOCTYPE specified in the document.
        /// </summary>
        public virtual DocType DocType { get; internal set; }

        /// <summary>
        /// Collection of ranges representing HTML comments.
        /// </summary>
        public CommentCollection CommentCollection { get; set; }

        /// <summary>
        /// A service that helps determine if &lt;script> block contains
        /// code or markup. For example script block with type="text/xml"
        /// contains XML and script block with type="text/x-handlebars" contains
        /// HTML fragment.
        /// </summary>
        public IHtmlScriptTypeResolutionService ScriptTypeResolution { get; private set; }

        /// <summary>
        ///  Provides tag names for script and style tags.
        /// </summary>
        public IHtmlScriptOrStyleTagNamesService ScriptOrStyleTagNameService { get; private set; }

        /// <summary>
        /// Specifies if parser is in HTML, XML or XHTMLmode. XHTML is case-sensitive so parser won't match 
        /// &lt;html&gt; to &lt;/HTML&gt; In XML mode no special handling is provided for &lt;script> or 
        /// &lt;style> blocks or attributes.
        /// </summary>
        public ParsingMode ParsingMode { get; private set; }

        /// <summary>
        /// An object that provides information if a particular element
        /// is a self-closing element like &lt;br /&gt; or if it can be
        /// implictly closed like &lt;td&gt; or &lt;li&gt;.
        /// </summary>
        internal HtmlClosureProvider HtmlClosureProvider { get; set; }

        public bool CaseSensitive {
            get { return ParsingMode != ParsingMode.Html; }
        }

        public StringComparison StringComparison {
            get { return this.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal; }
        }

        public IEqualityComparer<string> StringComparer {
            get { return this.IgnoreCase ? System.StringComparer.OrdinalIgnoreCase : System.StringComparer.Ordinal; }
        }

        public bool IgnoreCase {
            get { return ParsingMode == ParsingMode.Html; }
        }

        #region Constructors
        public HtmlTree(ITextProvider text)
            : this(text, null, null, ParsingMode.Html) {
        }

        public HtmlTree(ITextProvider text, IHtmlScriptTypeResolutionService scriptTypeResolution)
            : this(text, scriptTypeResolution, null, ParsingMode.Html) {
        }

        public HtmlTree(ITextProvider text, IHtmlScriptTypeResolutionService scriptTypeResolution, 
                        IHtmlScriptOrStyleTagNamesService scriptOrStyleTagNameService, ParsingMode parsingMode) {
            Text = text;

            ScriptTypeResolution = scriptTypeResolution;
            ScriptOrStyleTagNameService = (scriptOrStyleTagNameService ?? new DefaultScriptOrStyleTagNameService());
            ParsingMode = parsingMode;

            HtmlClosureProvider = new HtmlClosureProvider();
            CommentCollection = new CommentCollection();

            // Create root node last when all fields are intialized
            RootNode = new RootNode(this);
        }
        #endregion

        /// <summary>
        /// Main method to build the tree.
        /// </summary>
        public virtual void Build() {
            Build(TextRange.FromBounds(0, Text.Length));
        }

        /// <summary>
        /// Main method to build a subtree from a text range.
        /// </summary>
        public virtual void Build(ITextRange range) {
            CommentCollection = new CommentCollection();

            HtmlParser parser = new HtmlParser(ParsingMode, ScriptTypeResolution, ScriptOrStyleTagNameService);
            HtmlTreeBuilder builder = new HtmlTreeBuilder(parser, this);
            parser.Parse(Text, range);

            DocType = parser.DocType;
            ParsingMode = parser.ParsingMode;
        }

        #region IHtmlTreeVisitorPattern Members
        public bool Accept(IHtmlTreeVisitor visitor, object param) {
            return RootNode.Accept(visitor, param);
        }

        public bool Accept(Func<ElementNode, object, bool> visitor, object param) {
            return RootNode.Accept(visitor, param);
        }
        #endregion

        /// <summary>
        /// Updates positions of all elements and attributes in the tree
        /// reflecting change made to the source text buffer.
        /// </summary>
        /// <param name="start">Start position of the change</param>
        /// <param name="oldLength">Length of changed fragment before the change</param>
        /// <param name="newLength">Length of changed fragment after the change</param>
        public void ReflectTextChanges(IList<TextChangeEventArgs> textChanges) {
            if (textChanges.Count == 1) {
                ReflectTextChange(textChanges[0].Start, textChanges[0].OldLength, textChanges[0].NewLength);
            } else if (textChanges.Count > 1) {
                // See if we can merge the edits into a single ReflectTextChange call
                TextChangeEventArgs firstChange = textChanges[0];
                TextChangeEventArgs lastChange = textChanges[textChanges.Count - 1];
                int start = firstChange.Start;
                int oldLength = (lastChange.OldStart - firstChange.OldStart) + lastChange.OldLength;
                bool allowMerge = true;
                int firstItemAtOrAfter;

                if (allowMerge) {
                    firstItemAtOrAfter = CommentCollection.GetFirstItemAfterOrAtPosition(start);
                    if ((firstItemAtOrAfter >= 0) && (CommentCollection[firstItemAtOrAfter].Start <= start + oldLength)) {
                        allowMerge = false;
                    }
                }

                if (allowMerge) {
                    ElementNode element = RootNode.ElementFromPosition(start);
                    if ((element == null) || (element.InnerRange.Start > start) || (element.InnerRange.End < (start + oldLength))) {
                        allowMerge = false;
                    } else if (element.Children.Count != 0) {
                        allowMerge = false;
                    }
                }

                if (allowMerge) {
                    int newLength = (lastChange.Start - firstChange.Start) + lastChange.NewLength;
                    ReflectTextChange(start, oldLength, newLength);
                } else {
                    foreach (TextChangeEventArgs curChange in textChanges) {
                        ReflectTextChange(curChange.Start, curChange.OldLength, curChange.NewLength);
                    }
                }
            }
        }

        public void ReflectTextChange(int start, int oldLength, int newLength) {
            // Note that shifting tree elements also shifts artifacts in 
            // element attributes. We need to track these changes in order
            // to avoid double shifts in artifacts.
            int offset = newLength - oldLength;
            RootNode.ShiftStartingFrom(start, offset);
            CommentCollection.ReflectTextChange(start, oldLength, newLength);
        }
    }
}
