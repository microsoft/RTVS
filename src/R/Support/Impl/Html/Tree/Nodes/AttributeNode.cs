// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Html.Core.Tree.Utility;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Tree.Nodes {
    /// <summary>
    /// HTML tree node that represents an attribute
    /// </summary>
    public class AttributeNode : TreeNode {
        string _name;
        internal static ReadOnlyCollection<AttributeNode> EmptyCollection = new ReadOnlyCollection<AttributeNode>(new AttributeNode[0]);

        public static AttributeNode Create(ElementNode parent, AttributeToken token) {
            var nameToken = token.NameToken as NameToken;

            if (nameToken != null && nameToken.HasPrefix())
                return new AttributeNodeWithPrefix(parent, token);

            return new AttributeNode(parent, token);

        }

        protected AttributeNode(ElementNode parent, AttributeToken token) {
            AttributeToken = token;

            if (parent != null && parent.Root.Tree != null) {
                var nameToken = token.NameToken as NameToken;

                if (token.HasName())
                    _name = parent.GetText(nameToken != null ? nameToken.NameRange : token);
                else
                    _name = String.Empty;

                UpdateValue(parent.TextProvider);
            }
        }

        /// <summary>
        /// Underlying parse token
        /// </summary>
        public AttributeToken AttributeToken { get; private set; }

        /// <summary>
        /// Attribute value token
        /// </summary>
        public IHtmlAttributeValueToken ValueToken { get { return AttributeToken.ValueToken; } }

        public override string Name { get { return _name; } }

        /// <summary>
        /// Node prefix
        /// </summary>
        public override string Prefix { get { return String.Empty; } }

        /// <summary>
        /// Node fully qialified name (prefix:name)
        /// </summary>
        public override string QualifiedName { get { return Name; } }

        /// <summary>
        /// Attribute value with quotes
        /// </summary>
        public string QuotedValue { get; private set; }

        /// <summary>
        /// Attribute value without quotes
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Determines if attribute is a standalone attribute (i.e. does not have equal sign or value)
        /// </summary>
        public bool IsStandalone() { return AttributeToken.EqualsSign < 0 && !AttributeToken.HasValue(); }

        /// <summary>
        /// Detemines if attribute has a value
        /// </summary>
        public bool HasValue() { return ValueToken != null && ValueToken.Length > 0; }

        /// <summary>
        /// Tells if attribute has an equal sign
        /// </summary>
        /// <returns>True if attribute has value</returns>
        public bool HasEqualSign() { return AttributeToken.HasEqualSign(); }

        /// <summary>
        /// Determines if attribute name has namespace prefix
        /// </summary>
        public override bool HasPrefix() { return (Prefix != null && Prefix.Length > 0) || AttributeToken.HasPrefix(); }

        /// <summary>
        /// Determines of attribute has name
        /// </summary>
        public bool HasName() { return (Name != null && Name.Length > 0) || AttributeToken.HasName(); }

        /// <summary>
        /// Determines if attribute has qualified name (i.e. both prefix and name)
        /// </summary>
        public bool HasQualifiedName() { return (QualifiedName != null && QualifiedName.Length > 0) || AttributeToken.HasQualifiedName(); }

        /// <summary>
        /// Text range of the attribute name's prefix
        /// </summary>
        public override ITextRange PrefixRange { get { return NameToken != null ? NameToken.PrefixRange : TextRange.EmptyRange; } }

        /// <summary>
        /// Text range of the attribute name
        /// </summary>
        public override ITextRange NameRange { get { return NameToken != null ? NameToken.NameRange : TextRange.EmptyRange; } }

        /// <summary>
        /// Text range of the attribute qualified name
        /// </summary>
        public override ITextRange QualifiedNameRange {
            get {
                if (NameToken != null) {
                    if (NameToken.HasQualifiedName())
                        return NameToken.QualifiedName;

                    if (NameToken.HasPrefix())
                        return NameToken.PrefixRange;

                    if (NameToken.HasName())
                        return NameToken.NameRange;
                }
                return TextRange.EmptyRange;
            }
        }

        /// <summary>
        /// Text range of the attribute equal sign (if present)
        /// </summary>
        public ITextRange EqualsSignRange {
            get {
                if (AttributeToken.EqualsSign >= 0)
                    return new TextRange(AttributeToken.EqualsSign, 1);

                return TextRange.EmptyRange;
            }
        }

        /// <summary>
        /// Text range of the attribute value (if present)
        /// </summary>
        public ITextRange ValueRange {
            get {
                if (AttributeToken.HasValue())
                    return AttributeToken.ValueToken;

                return TextRange.EmptyRange;
            }
        }

        /// <summary>
        /// Text range of the attribute value (if present), 
        /// not including quotes (if present)
        /// </summary>
        public ITextRange ValueRangeUnquoted {
            get {
                ITextRange valueRangeQuoted = ValueRange;

                if (valueRangeQuoted.Length > 0) {
                    int startOffset = ValueToken.OpenQuote != '\0' ? 1 : 0;
                    int endOffset = ValueToken.CloseQuote != '\0' ? 1 : 0;

                    int start = valueRangeQuoted.Start + startOffset;
                    int length = valueRangeQuoted.Length - startOffset - endOffset;

                    return new TextRange(start, length);
                }

                return valueRangeQuoted;
            }
        }

        /// <summary>
        /// Attribute name token
        /// </summary>
        public NameToken NameToken { get { return AttributeToken.NameToken as NameToken; } }

        /// <summary>
        /// Determines if attrubute value contains artifacts
        /// </summary>
        /// <returns></returns>
        public bool ValueContainsArtifacts() {
            if (AttributeToken.ValueToken != null) {
                var composite = AttributeToken.ValueToken as CompositeToken;
                if (composite != null) {
                    foreach (var token in composite) {
                        if (token.TokenType == HtmlTokenType.Artifact)
                            return true;
                    }
                }
            }

            return false;
        }

        #region Positions
        public override ITextRange InnerRange { get { return OuterRange; } }

        /// <summary>
        /// Start of the attribute text range
        /// </summary>
        public override int Start {
            get {
                if (AttributeToken != null)
                    return AttributeToken.Start;

                return 0;
            }
        }

        /// <summary>
        /// End of the attribute text range
        /// </summary>
        public override int End {
            get {
                if (AttributeToken != null)
                    return AttributeToken.End;

                return 0;
            }
        }

        public HtmlPositionType GetPositionType(int position) {
            if (QualifiedNameRange.Contains(position))
                return HtmlPositionType.AttributeName;

            if (this.EqualsSignRange.Length > 0) {
                if (position < EqualsSignRange.Start)
                    return HtmlPositionType.BeforeEqualsSign;

                if (EqualsSignRange.Start == position)
                    return HtmlPositionType.EqualsSign;
            }

            if (this.HasValue()) {
                if (position <= ValueRange.Start)
                    return HtmlPositionType.AfterEqualsSign;

                // Case like <a id="foo" href=| >
                if (position == ValueRange.End && this.ValueToken.CloseQuote == '\0') {
                    return HtmlPositionType.AttributeValue;
                }

                if (ValueRange.Contains(position)) {
                    if (this.IsStyleAttribute())
                        return HtmlPositionType.InInlineStyle;

                    if (this.IsScriptAttribute())
                        return HtmlPositionType.InInlineScript;

                    return HtmlPositionType.AttributeValue;
                }
            }

            if (EqualsSignRange.End == position)
                return HtmlPositionType.AfterEqualsSign;

            return HtmlPositionType.InStartTag;
        }

        #endregion

        #region Shifting
        public override void Shift(int offset) {
            AttributeToken.Shift(offset);
        }

        public override void ShiftStartingFrom(int position, int offset) {
            AttributeToken.ShiftStartingFrom(position, offset);
        }
        #endregion

        #region Attribute types
        /// <summary>Determines if attributes is a style attribute</summary>
        public bool IsStyleAttribute() {
            return String.Compare(Name, "style", StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>Determines if element is a script-type atribute (like onclick="")</summary>
        public bool IsScriptAttribute() {
            return !HasPrefix() && String.Compare(Name, 0, "on", 0, 2, StringComparison.OrdinalIgnoreCase) == 0;
        }
        #endregion

        internal void UpdateValue(ITextProvider textProvider) {
            if (this.HasValue()) {
                QuotedValue = textProvider.GetText(ValueToken);

                int start = ValueToken.Start + (ValueToken.OpenQuote != '\0' ? 1 : 0);
                int end = ValueToken.CloseQuote != '\0' ? ValueToken.End - 1 : ValueToken.End;
                int length = end - start;

                Value = length > 0 ? textProvider.GetText(new TextRange(start, length)) : String.Empty;
            } else {
                QuotedValue = String.Empty;
                Value = String.Empty;
            }
        }

        public override object Clone() {
            var clone = base.Clone() as AttributeNode;
            clone.AttributeToken = AttributeToken.Clone() as AttributeToken;

            return clone;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() {
            return AttributeToken.ToString();
        }
    }
}
