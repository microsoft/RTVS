// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser.Tokens {
    public class AttributeToken : IHtmlToken, ICompositeTextRange {
        public IHtmlToken NameToken { get; private set; } // not ItemName since it can be an ArtifactItem
        public int EqualsSign { get; private set; }
        public IHtmlAttributeValueToken ValueToken { get; protected set; }

        public AttributeToken(IHtmlToken nameToken, int equalsPosition, IHtmlAttributeValueToken value) {
            NameToken = nameToken;
            EqualsSign = equalsPosition;
            ValueToken = value;
        }

        public AttributeToken(IHtmlToken nameToken, int equalsPosition)
            : this(nameToken, equalsPosition, null) {
        }

        public AttributeToken(IHtmlToken nameToken)
            : this(nameToken, -1, null) {
        }

        /// <summary>
        /// Tells if attribute has a name
        /// </summary>
        /// <returns>True if attribute has name</returns>
        public bool HasName() {
            var nt = NameToken as NameToken;
            if (nt != null)
                return nt.HasName();

            return false;
        }

        /// <summary>
        /// Tells if attribute has an equal sign
        /// </summary>
        /// <returns>True if attribute has value</returns>
        public bool HasEqualSign() {
            return EqualsSign >= 0;
        }

        /// <summary>
        /// Determines if attribute name has prefix
        /// </summary>
        /// <returns>True if attribute name has prefix</returns>
        public bool HasPrefix() {
            var nt = NameToken as NameToken;
            return nt != null ? nt.HasPrefix() : false;
        }

        /// <summary>
        /// Determines if attribute name is a fully qualified name (prefix:name)
        /// </summary>
        /// <returns>True if both prefix and name are present</returns>
        public bool HasQualifiedName() {
            var nt = NameToken as NameToken;
            return nt != null ? nt.HasQualifiedName() : false;
        }

        /// <summary>
        /// Tells if attribute has a value
        /// </summary>
        /// <returns>True if attribute has value</returns>
        public bool HasValue() {
            return ValueToken != null;
        }

        #region IToken<HtmlTokenType> Members

        public HtmlTokenType TokenType {
            get { return HtmlTokenType.AttributeName; }
        }

        #endregion

        #region IHtmlToken
        public bool IsWellFormed {
            get { return true; }
        }
        #endregion

        #region ITextRange Members

        /// <summary>
        /// Attribute start position in the text buffer
        /// </summary>
        public int Start {
            get {
                if (NameToken != null)
                    return NameToken.Start;

                if (EqualsSign >= 0)
                    return EqualsSign;

                if (ValueToken != null)
                    return ValueToken.Start;

                return 0;
            }
        }

        public int End {
            get {
                if (ValueToken != null)
                    return ValueToken.End;

                if (EqualsSign >= 0)
                    return EqualsSign + 1;

                if (NameToken != null)
                    return NameToken.End;

                return 0;
            }
        }

        public int Length {
            get { return End - Start; }
        }

        public bool Contains(int position) {
            return TextRange.Contains(this, position);
        }

        public void Shift(int offset) {
            if (NameToken != null)
                NameToken.Shift(offset);

            if (EqualsSign >= 0)
                EqualsSign += offset;

            if (ValueToken != null)
                ValueToken.Shift(offset);

        }
        #endregion

        #region ICompositeTextRange

        public void ShiftStartingFrom(int start, int offset) {
            if (NameToken != null && NameToken.Start >= start) {
                Debug.Assert(!(start > NameToken.Start && start < NameToken.End), "Can't shift when start position is in the middle of a token");
                NameToken.Shift(offset);
            }

            if (EqualsSign >= start)
                EqualsSign += offset;

            if (ValueToken != null) {
                if (ValueToken.Start >= start) {
                    var composite = ValueToken as CompositeAttributeValueToken;
                    if (composite != null) {
                        composite.ShiftStartingFrom(start, offset);
                    } else if (ValueToken.Start >= start) {
                        ValueToken.Shift(offset);
                    }
                } else if (ValueToken.Contains(start)) {
                    var expandable = ValueToken as IExpandableTextRange;

                    if (expandable != null)
                        expandable.Expand(0, offset);
                }
            }
        }

        #endregion

        [ExcludeFromCodeCoverage]
        public override string ToString() {
            if (NameToken != null && ValueToken != null) {
                return String.Format(CultureInfo.CurrentCulture, "{0} = {1}", NameToken.ToString(), ValueToken.ToString());
            } else if (NameToken != null) {
                return NameToken.ToString();
            } else if (ValueToken != null) {
                return ValueToken.ToString();
            }

            return "?";
        }
    }
}
