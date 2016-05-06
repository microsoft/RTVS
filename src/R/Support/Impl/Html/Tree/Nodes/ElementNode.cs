// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.Html.Core.Parser;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Html.Core.Tree.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Utility;

namespace Microsoft.Html.Core.Tree.Nodes {
    /// <summary>
    /// Represent HTML element node in the tree
    /// </summary>
    public class ElementNode : TreeNode, ICompositeTextRange, IHtmlTreeVisitorPattern, IPropertyOwner {
        private static TextRangeCollection<TagNode> _emptyEndTagCollection = new TextRangeCollection<TagNode>();
        internal static ReadOnlyCollection<ElementNode> EmptyCollection = new ReadOnlyCollection<ElementNode>(new ElementNode[0]);

        /// <summary>
        /// Tree root node. Useful if you need to get tree-level information 
        /// like associated text provider and such.
        /// </summary>
        public virtual RootNode Root { get { return Parent != null ? Parent.Root : null; } }

        /// <summary>
        /// Parent node
        /// </summary>
        public ElementNode Parent { get; internal set; }

        /// <summary>Start tag node</summary>
        public TagNode StartTag { get; internal set; }

        /// <summary>End Tag Node</summary>
        public TagNode EndTag { get; internal set; }

        /// <summary>Is this the root element in the tree?</summary>
        public virtual bool IsRoot { get { return false; } }

        /// <summary>Collection of child nodes</summary>
        public virtual ReadOnlyCollection<ElementNode> Children { get; internal set; }

        /// <summary>Element attribute nodes</summary>
        public ReadOnlyCollection<AttributeNode> Attributes { get { return StartTag.Attributes; } }

        /// <summary>Element parent in the linked (master) document. Typically used in master/content
        /// page scenario when content page element parent is not the document root but
        /// rather element in the parent master page tree</summary>
        public ElementNode MasterParent { get; internal set; }

        /// <summary>
        /// Element unique key. Helps track elements in the tree as they come and go.
        /// For example, validation thread uses this to see if element it is about
        /// to validate is still in the tree or if it is already gone (deleted).
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// Collection of orphaned end tags that reside within element inner range
        /// </summary>
        public ReadOnlyTextRangeCollection<TagNode> OrphanedEndTags {
            get {
                if (OrphanedEndTagsCollection != null)
                    return new ReadOnlyTextRangeCollection<TagNode>(OrphanedEndTagsCollection);

                return new ReadOnlyTextRangeCollection<TagNode>(_emptyEndTagCollection);
            }
        }

        /// <summary>
        /// True if some of the node children were removed as a result
        /// of destructive editing. Used in incremental tree updates.
        /// </summary>
        internal bool ChildrenInvalidated { get; set; }

        // Stores 'max' position in the text buffer when element is not closed. Typically 'max'
        // is the text buffer length and it is the point where element start, end tag or element
        // content virtually ends when tag is not closed yet.
        protected int VirtualEnd { get; set; }

        /// <summary>
        /// True if element has ASP.NET runat="server" attribute
        /// </summary>
        private bool _hasRunatServerAttribute = false;

        /// <summary>
        /// Collection of orphaned end tags that appear in the element inner range.
        /// </summary>
        protected TextRangeCollection<TagNode> OrphanedEndTagsCollection { get; set; }

        public ElementNode(ElementNode parent, int openAngleBracketPosition, NameToken nameToken, int maxEnd) {
            Parent = parent;

            if (nameToken.HasColon)
                StartTag = new TagNodeWithPrefix(this, openAngleBracketPosition, nameToken, maxEnd);
            else
                StartTag = new TagNode(this, openAngleBracketPosition, nameToken, maxEnd);

            VirtualEnd = maxEnd;
            Properties = new PropertyDictionary();
        }

        #region Text utility methods
        public virtual string GetText(ITextRange range) {
            return range.Length > 0 ? Root.GetText(range) : String.Empty;
        }

        public virtual ITextProvider TextProvider { get { return Root.Tree.Text; } }
        #endregion

        public void AddOrphanedEndTag(TagNode tag) {
            if (OrphanedEndTagsCollection == null)
                OrphanedEndTagsCollection = new TextRangeCollection<TagNode>();

            OrphanedEndTagsCollection.Add(tag);
        }

        /// <summary>
        /// Node name
        /// </summary>
        public override string Name { get { return StartTag.Name; } }

        /// <summary>
        /// Node prefix
        /// </summary>
        public override string Prefix { get { return StartTag.Prefix; } }

        /// <summary>
        /// Node fully qialified name (prefix:name)
        /// </summary>
        public override string QualifiedName { get { return StartTag.QualifiedName; } }

        #region Positions
        /// <summary>Position of an element open angle bracket (start tag start position)</summary>
        public override int Start {
            get { return StartTag.Start; }
        }
        /// <summary>Element end position: end of start tag if element is short-hand or self closed
        /// or element end tag end if element has content and is well-formed.</summary>
        public override int End {
            get {
                if (IsShorthand() || StartTag.IsSelfClosing)
                    return StartTag.End;

                // When element is not a shorthand and there is no end tag it is either 
                // implicitly closed like <td> or <li> or end tag is missing 
                // (and element is closed at the EOF).

                if (EndTag != null)
                    return EndTag.End;

                // Not implictly closed and there is no end tag either
                return VirtualEnd;
            }
        }

        /// <summary>Element text range from start of start tag to the end of end tag</summary>
        public override ITextRange InnerRange {
            get {
                if (IsShorthand())
                    return TextRange.EmptyRange;

                if (EndTag != null)
                    return TextRange.FromBounds(StartTag.End, EndTag.Start);

                return TextRange.FromBounds(StartTag.End, OuterRange.End);
            }
        }
        #endregion

        /// <summary>Text range of the element name (bar in foo:bar)</summary>
        public override ITextRange NameRange { get { return StartTag.NameRange; } }

        /// <summary>Text range of the element prefix (foo in foo:bar)</summary>
        public override ITextRange PrefixRange { get { return StartTag.PrefixRange; } }

        public override bool HasPrefix() { return Prefix != null && Prefix.Length > 0; }

        /// <summary>Text range of the element qualified name (complete foo:bar)</summary>
        public override ITextRange QualifiedNameRange { get { return StartTag.QualifiedNameRange; } }

        /// <summary>Tells if element is a short-hand element ending in /&gt;</summary>
        public bool IsShorthand() { return StartTag.IsShorthand; }

        /// <summary>
        /// Tells if element is self-closing element like &lt;br&gt;. Does not tell if
        /// element is closed by />, use IsShortHand for that.
        /// </summary>
        public bool IsSelfClosing() { return StartTag.IsSelfClosing; }

        /// <summary>
        /// Determines if element contains given position
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        public override bool Contains(int position) {
            if (base.Contains(position)) {
                return true;
            }

            if (StartTag.Contains(position)) {
                return true;
            }

            if (EndTag != null) {
                if (EndTag.Contains(position)) {
                    return true;
                }
            } else {
                if (position == VirtualEnd && !IsShorthand() && !IsSelfClosing()) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Begins element end tag processing. Called exclusively by tree builder.
        /// when it discovers &lt;/name sequence.</summary>
        internal void OpenEndTag(int position, NameToken nameToken, int maxEnd) {
            EndTag = new TagNode(this, position, nameToken, maxEnd);
        }

        /// <summary>Completes element. Called by tree builder when element is complete:
        /// start tag is self-closed or implicitly closed or element is closed by
        /// a well-formed end tag or by another tag that is being opened.</summary>
        internal virtual void CompleteElement(
            ITextRange closingSequence,
            bool isClosed,
            ReadOnlyCollection<ElementNode> children,
            ReadOnlyCollection<AttributeNode> startTagAtributes,
            ReadOnlyCollection<AttributeNode> endTagAttributes) {
            Debug.Assert(children != null);
            Debug.Assert(startTagAtributes != null);
            Debug.Assert(endTagAttributes != null);

            if (!StartTag.IsComplete) {
                StartTag.Complete(startTagAtributes, closingSequence, isClosed, false, false);
            } else if (EndTag != null && !EndTag.IsComplete) {
                EndTag.Complete(endTagAttributes, closingSequence, isClosed, false, false);
            }

            Children = children;
            VirtualEnd = closingSequence.End;
        }

        /// <summary>
        /// Determines position type and enclosing element node for a given position in the document text.
        /// </summary>
        /// <param name="position">Position in the document text</param>
        /// <param name="element">Element that contains position</param>
        /// <param name="attribute">Attribute that contains position (may be null)</param>
        /// <returns>Position type as a set of flags combined via OR operation</returns>
        public virtual HtmlPositionType GetPositionElement(int position, out ElementNode element, out AttributeNode attribute) {
            element = null;
            attribute = null;

            // If start tag is not closed, consider end position to be inside it
            // so user can continue getting attribute intellisense, like in <a href=|<a ...></a>
            if (StartTag.Contains(position) || (position == StartTag.End && !StartTag.IsClosed)) {
                // If position is right at the start, it is actually before the tag (in parent's content), 
                // as if when caret position is like this: <table>|<tr></tr><table>
                if (position == StartTag.Start) {
                    element = this.Parent;
                    return HtmlPositionType.InContent;
                }

                if (position >= QualifiedNameRange.Start && position <= StartTag.QualifiedNameRange.End) {
                    element = this;
                    return HtmlPositionType.ElementName;
                }

                element = this;

                for (int i = 0; i < Attributes.Count; i++) {
                    var attrNode = Attributes[i];
                    bool hasClosingQuote = false;

                    var valueToken = attrNode.ValueToken;
                    hasClosingQuote = (valueToken != null) && (valueToken.CloseQuote != '\0');

                    if (position == attrNode.End && hasClosingQuote)
                        break;

                    if (position > attrNode.End)
                        continue;

                    if (position < attrNode.Start)
                        break;

                    if (attrNode.Contains(position) || (position == attrNode.End && !hasClosingQuote)) {
                        attribute = attrNode;
                        return attrNode.GetPositionType(position);
                    }
                }

                return HtmlPositionType.InStartTag;
            }

            if (!this.Contains(position))
                return HtmlPositionType.Undefined;

            for (int i = 0; i < this.Children.Count; i++) {
                var child = Children[i];

                if (position < child.Start)
                    break;

                if (child.Contains(position))
                    return child.GetPositionElement(position, out element, out attribute);
            }

            element = this;

            // If position is right at the start, it is actually before the end tag, 
            // like when caret is between opening and closing tags: <table>|<table>
            if (EndTag != null) {
                if (position == EndTag.Start)
                    return HtmlPositionType.InContent;

                if (EndTag.Contains(position))
                    return HtmlPositionType.InEndTag;
            }

            if (this.IsScriptBlock() && !this.IsMarkupScriptBlock())
                return HtmlPositionType.InScriptBlock;

            if (this.IsStyleBlock())
                return HtmlPositionType.InStyleBlock;

            return HtmlPositionType.InContent;
        }

        /// <summary>
        /// Retrieves node that contains both node1 and node2 ranges. Node1 range must 
        /// precede node2 in the text buffer
        /// </summary>
        /// <param name="node1">First node</param>
        /// <param name="node2">Second node</param>
        /// <returns>Node that contains both ranges</returns>
        public ElementNode GetCommonAncestor(ElementNode node1, ElementNode node2) {
            if (node1.Start > this.End || node2.End < this.Start)
                return null;

            if (node1 is RootNode)
                return node1;

            if (node2 is RootNode)
                return node2;

            for (int i = 0; i < this.Children.Count; i++) {
                var child = Children[i];

                if (child.Start > node2.End)
                    break;

                if (child.Contains(node1.Start) && child.End >= node2.End)
                    return child.GetCommonAncestor(node1, node2);
            }

            return this;
        }

        /// <summary>
        /// Given list of end tag names finds topmost element that matches provided end tags
        /// </summary>
        /// <param name="tagNames"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        internal ElementNode FindTopmostParent(List<string> tagNames, bool ignoreCase) {
            ElementNode topmostNode = this;
            var node = this;

            if (tagNames.Count > 1)
                return Root; // Edge case, can be optimized later if necessary.

            foreach (string tag in tagNames) {
                while (!(node is RootNode)) {
                    if (String.Compare(tag, node.QualifiedName, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0) {
                        topmostNode = node.Parent;

                        // Don't break and instead continue up to the topmost
                        // matching node so we properly handle insertion of </a>
                        // into the innermost element in <a><a><a>|</a></a></a>
                    }

                    node = node.Parent;
                }
            }

            return topmostNode;
        }

        /// <summary>
        /// Given text range locates previous and next child nodes that surround the range
        /// </summary>
        /// <param name="parentNode">Parent node</param>
        /// <param name="range">Text range</param>
        /// <param name="child1">Index of the child node that precedes start of the range</param>
        /// <param name="child2">Index of the child that follows end of the range</param>
        public void GetChildrenSurroundingRange(ITextRange range, out int child1, out int child2) {
            child1 = -1;
            child2 = -1;

            // Linear search (can be made binary since children are normally sorted).
            // However, typically there are not that many of them to look through.
            for (int i = Children.Count - 1; i >= 0; i--) {
                if (Children[i].End <= range.Start) {
                    child1 = i;
                    break;
                }
            }

            for (int i = child1 + 1; i < Children.Count; i++) {
                if (Children[i].Start >= range.End) {
                    child2 = i;
                    break;
                }
            }
        }

        #region Item lookup

        /// <summary>
        /// Finds deepest element node that contains given position
        /// </summary>
        /// <param name="pos">Position</param>
        /// <returns>Element node node or null if not found</returns>
        public virtual ElementNode ElementFromPosition(int pos) {
            if (!this.Contains(pos)) {
                return null; // not this element
            }

            for (int i = 0; i < this.Children.Count; i++) {
                var child = Children[i];

                if (child.Start > pos)
                    break;

                if (child.Contains(pos))
                    return child.ElementFromPosition(pos);
            }

            return this as ElementNode;
        }

        /// <summary>
        /// Finds deepest element node that fully encloses given range
        /// </summary>
        public ElementNode ElementFromRange(ITextRange range) {
            TreeNode item = null;

            if (TextRange.Contains(this, range)) {
                item = this;

                for (int i = 0; i < this.Children.Count; i++) {
                    var child = Children[i];

                    if (range.End < child.Start)
                        break;

                    if (child.Contains(range.Start) && child.Contains(range.End)) {
                        item = (child.Children.Count > 0)
                            ? child.ElementFromRange(range)
                            : child;

                        break;
                    }
                }
            }

            return item as ElementNode;
        }

        public bool Contains(ITextRange range, bool inclusiveTagStart, bool inclusiveTagEnd) {
            bool containsRange = false;
            if (this.Start < range.Start || (this.Start == range.Start && inclusiveTagStart)) {
                if (this.End > range.End || (this.End == range.End && inclusiveTagEnd))
                    containsRange = true;
            }

            return containsRange;
        }

        public ElementNode ElementFromRangeAndInclusion(ITextRange range, bool inclusiveTagStart, bool inclusiveTagEnd) {
            TreeNode item = null;

            if (this.Contains(range, inclusiveTagStart, inclusiveTagEnd)) {
                item = this;

                for (int i = 0; i < this.Children.Count; i++) {
                    var child = Children[i];

                    if (range.End < child.Start)
                        break;

                    if (child.Contains(range, inclusiveTagStart, inclusiveTagEnd)) {
                        item = (child.Children.Count > 0)
                            ? child.ElementFromRangeAndInclusion(range, inclusiveTagStart, inclusiveTagEnd)
                            : child;

                        break;
                    }
                }
            }

            return item as ElementNode;
        }

        /// <summary>
        /// Finds deepest element node which inner range fully encloses given positions
        /// </summary>
        /// <param name="range">Range to analyze</param>
        /// <param name="inclusiveEnd">If true, element and range may have same end points</param>
        public ElementNode ElementEnclosingRange(ITextRange range, bool inclusiveEnd) {
            ElementNode item = null;

            if (!this.IsShorthand() && TextRange.Contains(this.InnerRange, range, inclusiveEnd)) {
                item = this;

                foreach (var child in Children) {
                    if (range.End < child.Start)
                        break;

                    var element = child.ElementEnclosingRange(range, inclusiveEnd);
                    if (element != null) {
                        item = element;
                        break;
                    }
                }
            }

            return item;
        }
        #endregion

        #region ICompositeTextRange

        /// <summary>Shifts node start, end and all child elements by the specified offset.</summary>
        public override void Shift(int offset) {
            StartTag.Shift(offset);

            if (EndTag != null)
                EndTag.Shift(offset);

            if (OrphanedEndTagsCollection != null)
                OrphanedEndTagsCollection.Shift(offset);

            VirtualEnd += offset;

            for (int i = 0; i < Children.Count; i++) {
                Children[i].Shift(offset);
            }
        }

        /// <summary>
        /// Shifts node components that are located at or beyond given start point by the specified range
        /// </summary>
        /// <param name="start">Start point</param>
        /// <param name="offset">Offset to shift by</param>
        public override void ShiftStartingFrom(int start, int offset) {
            // short-circuit in the case where the shift starts after our end
            if (start > End)
                return;

            // if tag is not closed and change is right at the end,
            // e need to grow start tag rather than element inner range

            if (StartTag.Contains(start) || StartTag.Start >= start || (!StartTag.IsClosed && start == StartTag.End))
                StartTag.ShiftStartingFrom(start, offset);

            if (EndTag != null && (EndTag.Contains(start) || EndTag.Start >= start))
                EndTag.ShiftStartingFrom(start, offset);

            if (OrphanedEndTagsCollection != null)
                OrphanedEndTagsCollection.ShiftStartingFrom(start, offset);

            if (VirtualEnd >= start)
                VirtualEnd = Math.Max(start, VirtualEnd + offset);

            int count = Children.Count;
            for (int i = 0; i < count; i++) {
                Children[i].ShiftStartingFrom(start, offset);
            }
        }
        #endregion

        #region IPropertyOwner
        public PropertyDictionary Properties {
            get;
            private set;
        }
        #endregion

        #region Element types
        /// <summary>Determines if element is a script block</summary>
        public bool IsScriptBlock() {
            if (Root.ParsingMode == ParsingMode.Xml)
                return false;

            bool isScript;
            if (Root.Tree != null) {
                StringComparer comparer = Root.ParsingMode == ParsingMode.Html ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
                IReadOnlyList<string> scriptTagNames = Root.Tree.ScriptOrStyleTagNameService.GetScriptTagNames();
                isScript = scriptTagNames.Contains(QualifiedName, comparer);
            } else {
                StringComparison comparison = Root.ParsingMode == ParsingMode.Html ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                isScript = QualifiedName.Equals("script", comparison);
            }

            return isScript;
        }

        /// <summary>
        /// Determines if script block contains markup rather than code.
        /// </summary>
        public bool IsMarkupScriptBlock() {
            if (IsScriptBlock() && !IsRunatServer()) {
                var tree = this.Root.Tree;
                if (tree != null) {
                    var typeAttribute = GetAttribute("type", this.Root.Tree.IgnoreCase);
                    if (typeAttribute != null && typeAttribute.HasValue()) {
                        return tree.ScriptTypeResolution.IsScriptContentMarkup(typeAttribute.Value);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if script block is a JavaScript block.
        /// </summary>
        public bool IsJavaScriptBlock() {
            if (IsClientScriptBlock()) {
                var tree = this.Root.Tree;
                if (tree != null) {
                    var typeAttribute = GetAttribute("type", tree.IgnoreCase);
                    if (typeAttribute != null && typeAttribute.HasValue()) {
                        return tree.ScriptTypeResolution.IsScriptContentJavaScript(typeAttribute.Value);
                    } else {
                        // Script blocks with no "type" attribute or with no value for that attribute will
                        // be classified as JavaScript
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if script block contains client script code.
        /// </summary>
        public bool IsClientScriptBlock() {
            return IsScriptBlock() && !IsRunatServer() && !IsMarkupScriptBlock();
        }

        /// <summary>Determines if element is a style block</summary>
        public bool IsStyleBlock() {
            if (Root.ParsingMode == ParsingMode.Xml)
                return false;

            var comparison = Root.ParsingMode == ParsingMode.Html ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return String.Compare(Name, "style", comparison) == 0;
        }

        /// <summary>Determines if element has runat="server" attribute</summary>
        public bool IsRunatServer() {
            return _hasRunatServerAttribute;
        }

        /// <summary>Determines if element is an ASP.NET control</summary>
        public bool IsAspNetControl() {
            if (Root.ParsingMode == ParsingMode.Xml)
                return false;

            return GetAttribute("id", ignoreCase: true) != null && this.IsRunatServer();
        }

        /// <summary>Determines if element is a DOCTYPE element</summary>
        public bool IsDocType() {
            return !HasPrefix() && String.Compare(Name, "!doctype", StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>Determines if element is an XML Processing Instruction</summary>
        public bool IsXmlPi() {
            return !HasPrefix() && String.Compare(Name, "?xml", StringComparison.Ordinal) == 0;
        }

        public string Id {
            get {
                var idAttribute = GetAttribute("id", Root.Tree.IgnoreCase);
                if (idAttribute != null && idAttribute.HasValue())
                    return idAttribute.Value;

                return String.Empty;
            }
        }
        #endregion

        #region IHtmlTreeVisitorPattern Members
        public virtual bool Accept(IHtmlTreeVisitor visitor, object parameter) {
            if (visitor != null && visitor.Visit(this, parameter)) {
                for (int i = 0; i < this.Children.Count; i++) {
                    var child = Children[i];

                    if (!child.Accept(visitor, parameter))
                        return false;
                }

                return true;
            }

            return false;
        }

        public virtual bool Accept(Func<ElementNode, object, bool> visitor, object parameter) {
            if (visitor != null && visitor(this, parameter)) {
                foreach (ElementNode child in Children) {
                    if (!child.Accept(visitor, parameter))
                        return false;
                }

                return true;
            }

            return false;
        }
        #endregion

        public void RemoveChild(int index) {
            RemoveChildren(index, 1);
        }

        public void RemoveChildren(int start, int count) {
            if (start < 0 || start >= Children.Count)
                throw new ArgumentOutOfRangeException("start");

            if (count < 0 || count > Children.Count || start + count > Children.Count)
                throw new ArgumentOutOfRangeException("count");

            if (Children.Count == count) {
                Children = ElementNode.EmptyCollection;
            } else {
                var newChildren = new ElementNode[Children.Count - count];

                int j = 0;

                for (int i = 0; i < start; i++, j++)
                    newChildren[j] = Children[i];

                for (int i = start + count; i < Children.Count; i++, j++)
                    newChildren[j] = Children[i];

                Children = new ReadOnlyCollection<ElementNode>(newChildren);
            }
        }

        public void RemoveAllChildren() {
            Children = ElementNode.EmptyCollection;
        }

        /// <summary>
        /// Locates element attribute by name using string comparison as 
        /// specified by tree the element is in.
        /// </summary>
        /// <param name="attributeName">Fully qualified attribute name</param>
        /// <returns>Attribute or null if not found</returns>
        public AttributeNode GetAttribute(string attributeName) {
            return GetAttribute(attributeName, this.Root.IgnoreCase);
        }

        /// <summary>
        /// Locates element attribute by name
        /// </summary>
        /// <param name="attributeName">Fully qualified attribute name</param>
        /// <param name="ignoreCase">True if comparison should be case-insensitive</param>
        /// <returns>Attribute or null if not found</returns>
        public AttributeNode GetAttribute(string attributeName, bool ignoreCase) {
            int foundIndex = GetAttributeIndex(attributeName, ignoreCase);

            return foundIndex >= 0 ? Attributes[foundIndex] : null;
        }

        /// <summary>
        /// Locates element attribute by name
        /// </summary>
        /// <param name="attributeName">Fully qualified attribute name</param>
        /// <returns>index of attribute if found, else -1</returns>
        public int GetAttributeIndex(string attributeName) {
            return GetAttributeIndex(attributeName, this.Root.IgnoreCase);
        }

        /// <summary>
        /// Locates element attribute by name
        /// </summary>
        /// <param name="attributeName">Fully qualified attribute name</param>
        /// <param name="ignoreCase">True if comparison should be case-insensitive</param>
        /// <returns>index of attribute if found, else -1</returns>
        public int GetAttributeIndex(string attributeName, bool ignoreCase) {
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            for (int i = 0; i < this.Attributes.Count; i++) {
                var attribute = this.Attributes[i];

                if (attribute.HasName() && String.Compare(attribute.QualifiedName, attributeName, comparison) == 0)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Locates element attribute by prefix and name
        /// </summary>
        /// <param name="prefix">Namespace prefix</param>
        /// <param name="attributeName">Attribute name</param>
        /// <param name="ignoreCase">True if comparison should be case-insensitive</param>
        /// <returns>Attribute or null if not found</returns>
        public AttributeNode GetAttribute(string prefix, string attributeName, bool ignoreCase) {
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            for (int i = 0; i < this.Attributes.Count; i++) {
                var attribute = this.Attributes[i];

                if (attribute.HasPrefix() && !String.IsNullOrEmpty(prefix)) {
                    if (String.Compare(attribute.Prefix, prefix, comparison) == 0) {
                        if (attribute.HasName() && String.Compare(attribute.Name, attributeName, comparison) == 0)
                            return attribute;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns next element sibling or null if element is the last child or element is a root node
        /// </summary>
        public ElementNode NextSibling {
            get {
                if (!(this is RootNode)) {
                    for (int i = 0; i < Parent.Children.Count; i++) {
                        if (Parent.Children[i] == this)
                            return i == Parent.Children.Count - 1 ? null : Parent.Children[i + 1];
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Returns previous element sibling or null if element is the first child or element is a root node
        /// </summary>
        public ElementNode PreviousSibling {
            get {
                if (!(this is RootNode)) {
                    for (int i = 0; i < Parent.Children.Count; i++) {
                        if (Parent.Children[i] == this)
                            return i == 0 ? null : Parent.Children[i - 1];
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Clones tre node with or without children. Clone does not have positioning information. 
        /// Primary use of the close is HTML validation that runs in a background thread and only
        /// needs data like element name or attribute value.
        /// </summary>
        /// <param name="parent">Parent element to use. Must also be a clone.</param>
        /// <param name="cloneChildren">True if method should recurse into children nodes and clone them as well.</param>
        /// <returns>Element clone. Clone contains textual data but no positioning information.</returns>
        public virtual ElementNode Clone(ElementNode parent, bool cloneChildren) {
            var clone = base.Clone() as ElementNode;

            clone.Parent = parent;

            if (cloneChildren && Children.Count > 0) {
                var children = new ElementNode[cloneChildren ? Children.Count : 0];
                for (int i = 0; i < children.Length; i++) {
                    children[i] = Children[i].Clone(clone, cloneChildren) as ElementNode;
                }

                clone.Children = new ReadOnlyCollection<ElementNode>(children);
            } else {
                clone.Children = ElementNode.EmptyCollection;
            }

            clone.StartTag = StartTag.Clone() as TagNode;

            if (clone.EndTag != null)
                clone.EndTag = EndTag.Clone() as TagNode;

            clone.Properties = Properties;

            return clone;
        }

        /// <summary>
        /// Compares element qualified name to the provided one using
        /// string comparion rules as specified by the containing tree.
        /// </summary>
        public bool IsElement(string name) {
            return QualifiedName.Equals(name, this.Root.StringComparison);
        }

        /// <summary>
        /// Clones element without its children
        /// </summary>
        /// <param name="parent">Parent element to use. Must also be a clone.</param>
        /// <returns>Element clone. Clone contains textual data but no positioning information.</returns>
        public ElementNode CloneSingle(ElementNode parent) {
            return Clone(parent, false);
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "cloneChildren")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ElementNode")]
        public override object Clone() {
            throw new NotImplementedException("Call ElementNode.Clone(ElementNode parent, bool cloneChildren) instead");
        }

        internal void ClearOrphanedEndTags() {
            OrphanedEndTagsCollection = null;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() {
            if (EndTag != null)
                return String.Format(CultureInfo.CurrentCulture, "<{0}...{1}  [{2}...{3}]", QualifiedName, Root.GetText(EndTag.Start, EndTag.Length), Start, End);
            else if (this.IsShorthand())
                return String.Format(CultureInfo.CurrentCulture, "<{0}...{1}  [{2}...{3}", QualifiedName, Root.GetText(StartTag.End - 2, 2), Start, End);
            else if (this.StartTag.IsClosed)
                return String.Format(CultureInfo.CurrentCulture, "<{0}...{1}  [{2}...{3}", QualifiedName, Root.GetText(StartTag.End - 1, 1), Start, End);
            else
                return String.Format(CultureInfo.CurrentCulture, "<{0}...?  [{1}...{2}", QualifiedName, Start, End);
        }
    }
}
