// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser.Tokens {
    /// <summary>
    /// Token representing element or attribute name (prefix:name sequence)
    /// </summary>
    public class NameToken : IHtmlToken {
        public static NameToken Create(int prefixStart, int prefixLength, int colonPos, int nameStart, int nameLength) {
            if (prefixLength > 0 || colonPos >= 0)
                return new NameTokenWithPrefix(prefixStart, prefixLength, colonPos, nameStart, nameLength);

            return new NameToken(nameStart, nameLength);
        }

        public static NameToken Create(int nameStart, int nameLength) {
            return new NameToken(nameStart, nameLength);
        }

        #region Constructors
        protected NameToken(int nameStart, int nameLength) {
            NameRange = new TextRange(nameStart, nameLength);
        }

        public NameToken(TextRange nameRange) :
            this(nameRange.Start, nameRange.Length) {
        }
        #endregion

        /// <summary>
        /// Range of the item name
        /// </summary>
        public ITextRange NameRange { get; private set; }

        /// <summary>
        /// Range of the colon symbol (may be an invalid range)
        /// </summary>
        public virtual ITextRange ColonRange  => TextRange.EmptyRange;

        /// <summary>
        /// Range of the prefix (may be invalid range)
        /// </summary>
        public virtual ITextRange PrefixRange => TextRange.EmptyRange;

        public bool HasName() {
            return NameRange.Length > 0;
        }

        public bool HasColon => ColonRange.Length > 0;

        public bool HasPrefix() {
            return PrefixRange.Length > 0;
        }

        public bool HasQualifiedName() {
            return HasName() && HasPrefix() && HasColon;
        }

        public ITextRange QualifiedName {
            get {
                if (HasQualifiedName())
                    return TextRange.FromBounds(PrefixRange.Start, NameRange.End);

                if (HasPrefix())
                    return TextRange.FromBounds(PrefixRange.Start, ColonRange.End);

                if (HasName()) {
                    if (HasColon)
                        return TextRange.FromBounds(ColonRange.Start, NameRange.End);

                    return NameRange;
                }

                if (HasColon)
                    return ColonRange;

                return TextRange.EmptyRange;
            }
        }

        public bool IsNameWellFormed() {
            return HasName() && ((HasPrefix() && HasColon) || (!HasPrefix() && !HasColon));
        }

        public bool IsValid() {
            return Length > 0;
        }

        #region IToken<HtmlTokenType> Members

        public HtmlTokenType TokenType {
            get { return HtmlTokenType.ElementName; }
        }

        #endregion

        #region IHtmlToken
        public bool IsWellFormed {
            get { return true; }
        }
        #endregion

        #region ITextRange Members

        public int Start {
            get {
                if (HasPrefix())
                    return PrefixRange.Start;
                else if (HasColon)
                    return ColonRange.Start;
                else if (HasName())
                    return NameRange.Start;

                Debug.Fail("Cannot get position of a name token");
                return 0;
            }
        }

        public int End {
            get {
                if (HasName())
                    return NameRange.End;
                else if (HasColon)
                    return ColonRange.End;
                else if (HasPrefix())
                    return PrefixRange.End;

                Debug.Fail("Cannot get position of a name token");
                return 0;
            }
        }

        public int Length {
            get { return End - Start; }
        }

        public bool Contains(int position) {
            return TextRange.Contains(this, position);
        }

        public virtual void Shift(int offset) {
            if (NameRange.Length > 0)
                NameRange.Shift(offset);
        }

        #endregion

        [ExcludeFromCodeCoverage]
        public override string ToString() {
            if (PrefixRange.Length > 0)
                return String.Format(CultureInfo.CurrentCulture, "ItemName: {0}-{1}:{2}-{3} ", PrefixRange.Start, PrefixRange.End, NameRange.Start, NameRange.End);
            else
                return String.Format(CultureInfo.CurrentCulture, "ItemName: {0}-{1} ", NameRange.Start, NameRange.End);
        }
    }
}
