// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Html.Core.Parser;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Html.Core.Tree.Utility;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Tree.Nodes {
    public sealed class RootNode : ElementNode {
        private HtmlTree _owner;
        private ITextProvider _clonedProvider;
        private StringComparison _stringComparison;

        public RootNode(HtmlTree owner)
            : base(null, 0, NameToken.Create(0, 0), owner.Text.Length) {
            _owner = owner;
            Children = ElementNode.EmptyCollection;
        }

        public override ITextProvider TextProvider {
            get {
                if (Tree != null)
                    return Tree.Text;
                else if (_clonedProvider != null)
                    return _clonedProvider;
                else
                    return null;
            }
        }

        public HtmlTree Tree { get { return _owner; } }

        public override RootNode Root { get { return this; } }

        public override bool IsRoot { get { return true; } }

        public ParsingMode ParsingMode { get { return Tree != null ? Tree.ParsingMode : ParsingMode.Html; } }

        private DocType? _docType;
        public DocType DocType {
            get {
                if (_docType.HasValue) {
                    return _docType.Value;
                }

                foreach (var child in Children) {
                    if (child != null && child.IsDocType()) {
                        return DocTypeSignatures.GetDocType(child.GetText(child.OuterRange));
                    }
                }

                return DocType.Undefined;
            }

            private set {
                _docType = value;
            }
        }

        public override string Name {
            get { return String.Empty; }
        }

        public override string QualifiedName {
            get { return String.Empty; }
        }

        public override string Prefix {
            get { return String.Empty; }
        }

        public override int Start { get { return 0; } }

        public override int End {
            get { return _owner.Text.Length; }
        }

        public override ITextRange NameRange { get { return TextRange.EmptyRange; } }

        public override ITextRange InnerRange { get { return TextRange.FromBounds(Start, End); } }

        public override ITextRange OuterRange { get { return TextRange.FromBounds(Start, End); } }

        public override ITextRange PrefixRange { get { return TextRange.EmptyRange; } }

        public StringComparison StringComparison { get { return Tree != null ? Tree.StringComparison : _stringComparison; } }

        public IEqualityComparer<string> StringComparer {
            get {
                if (Tree != null) {
                    return Tree.StringComparer;
                }

                return IgnoreCase
                    ? System.StringComparer.OrdinalIgnoreCase
                    : System.StringComparer.Ordinal;
            }
        }

        public bool IgnoreCase { get { return Tree != null ? Tree.IgnoreCase : _stringComparison == StringComparison.OrdinalIgnoreCase; } }

        public string GetText(int start, int length) {
            if (Tree != null)
                return Tree.Text.GetText(new TextRange(start, Math.Min(Tree.Text.Length - start, length)));

            return String.Empty;
        }

        public override string GetText(ITextRange range) {
            if (Tree != null)
                return Tree.Text.GetText(range);

            return String.Empty;
        }

        /// <summary>
        /// Determines given position type. Returns element and attribute at this position. Attribute can be null.
        /// Return type specifies position as a set of flags <seealso cref="HtmlPositionType"/>
        /// </summary>
        /// <param name="position">Position in the document</param>
        /// <param name="element">Element that contains this position</param>
        /// <param name="attribute">Attribute that contains this position or null of none</param>
        /// <returns>Set of flags describing the position</returns>
        public override HtmlPositionType GetPositionElement(int position, out ElementNode element, out AttributeNode attribute) {
            element = null;
            attribute = null;

            foreach (ElementNode child in Children) {
                if (child.Start > position)
                    break;

                if (child.Contains(position))
                    return child.GetPositionElement(position, out element, out attribute);
            }

            element = this;
            return HtmlPositionType.InContent;
        }

        /// <summary>
        /// Finds deepest element node that contains given position
        /// </summary>
        /// <param name="pos">Position</param>
        /// <returns>Element node node or null if not found</returns>
        public override ElementNode ElementFromPosition(int pos) {
            ElementNode element = base.ElementFromPosition(pos);
            if (element == null)
                element = this;

            return element;
        }

        public override void ShiftStartingFrom(int start, int offset) {
            // Root node does not have start or end tags, only children
            for (int i = 0; i < Children.Count; i++) {
                Children[i].ShiftStartingFrom(start, offset);
            }

            if (OrphanedEndTagsCollection != null)
                OrphanedEndTagsCollection.ShiftStartingFrom(start, offset);
        }

        /// <summary>
        /// Finds two elements that surround given text range
        /// </summary>
        /// <param name="start">Range start</param>
        /// <param name="length">Range length</param>
        /// <param name="startNode">Element that precedes the range or null if there is none</param>
        /// <param name="startPositionType">Type of position in the start element</param>
        /// <param name="endNode">Element that follows the range or null if there is none</param>
        /// <param name="endPositionType">Type of position in the end element</param>
        /// <returns>Element that encloses the range or root node</returns>
        public ElementNode GetElementsEnclosingRange(
                                int start, int length,
                                out ElementNode startNode, out HtmlPositionType startPositionType,
                                out ElementNode endNode, out HtmlPositionType endPositionType) {
            int end = start + length;

            AttributeNode att1;
            startPositionType = GetPositionElement(start, out startNode, out att1);
            if (start != end) {
                AttributeNode att2;
                endPositionType = GetPositionElement(end, out endNode, out att2);
            } else {
                endNode = startNode;
                endPositionType = startPositionType;
            }

            if (startNode == endNode)
                return startNode;

            return this;
        }

        public override ElementNode Clone(ElementNode parent, bool cloneChildren) {
            var clone = base.Clone(null, cloneChildren) as RootNode;

            for (int i = 0; i < clone.Children.Count; i++) {
                clone.Children[i].Parent = clone;
            }

            clone._clonedProvider = clone._owner.Text;
            clone._stringComparison = clone._owner.StringComparison;

            clone._owner = null; // clones don't have trees
            clone.DocType = DocType;

            return clone;
        }


        [ExcludeFromCodeCoverage]
        public override string ToString() {
            return String.Format(CultureInfo.InvariantCulture, "Root: {0} children, [{1}...{2}", Children.Count, Start, End);
        }
    }
}
