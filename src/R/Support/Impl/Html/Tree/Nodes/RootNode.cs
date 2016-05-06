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
        private StringComparison _stringComparison;

        public RootNode(HtmlTree owner)
            : base(null, 0, NameToken.Create(0, 0), owner.Text.Length) {
            _owner = owner;
            Children = ElementNode.EmptyCollection;
        }

        public override ITextProvider TextProvider => Tree?.Text;
        public HtmlTree Tree => _owner;
        public override RootNode Root => this;
        public override bool IsRoot => true;
        public ParsingMode ParsingMode => Tree != null ? Tree.ParsingMode : ParsingMode.Html;

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

        public override string Name => string.Empty;
        public override string QualifiedName => string.Empty;
        public override string Prefix => string.Empty;
        public override int Start => 0;
        public override int End => _owner.Text.Length;

        public override ITextRange NameRange => TextRange.EmptyRange;
        public override ITextRange InnerRange => TextRange.FromBounds(Start, End);
        public override ITextRange OuterRange => TextRange.FromBounds(Start, End);
        public override ITextRange PrefixRange => TextRange.EmptyRange;
        public StringComparison StringComparison => Tree != null ? Tree.StringComparison : _stringComparison;

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

        [ExcludeFromCodeCoverage]
        public override string ToString() {
            return String.Format(CultureInfo.InvariantCulture, "Root: {0} children, [{1}...{2}", Children.Count, Start, End);
        }
    }
}
