// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser.Tokens {
    public class HtmlToken : BaseHtmlToken {
        private readonly bool _isWellFormed;
        private readonly HtmlTokenType _tokenType;

        public override bool IsWellFormed => _isWellFormed;

        public override HtmlTokenType TokenType => _tokenType;

        public HtmlToken(HtmlTokenType type)
            : this(type, 0, 0) {
        }

        public HtmlToken(HtmlTokenType type, int start, int length)
            : this(type, start, length, true) {
        }

        public HtmlToken(HtmlTokenType type, int start, int length, bool wellFormed)
            : base(start, length) {
            _tokenType = type;
            _isWellFormed = wellFormed;
        }

        public HtmlToken(int start, int length) :
            this(HtmlTokenType.Range, start, length) {
        }

        public static HtmlToken FromBounds(HtmlTokenType type, int start, int end, bool wellFormed) {
            return new HtmlToken(type, start, end - start, wellFormed);
        }

        public static HtmlToken FromBounds(HtmlTokenType type, int start, int end) {
            return new HtmlToken(type, start, end - start);
        }

        public static HtmlToken FromBounds(int start, int end) {
            return new HtmlToken(start, end - start);
        }
    }

    /// <summary>
    /// HTML parse token. Implements IToken interface.
    /// </summary>
    public abstract class BaseHtmlToken : IHtmlToken, ICloneable, IExpandableTextRange {
        public abstract bool IsWellFormed { get; }

        private int _start;
        private int _end;

        public BaseHtmlToken(int start, int length) {
            _start = start;
            _end = start + length;
        }

        public virtual object Clone() {
            return this.MemberwiseClone();
        }

        #region IToken<HtmlTokenType> Members

        public abstract HtmlTokenType TokenType {
            get;
        }

        #endregion

        #region ITextRange Members

        public virtual int Start {
            get { return _start; }
        }

        public virtual int End {
            get { return _end; }
        }

        public int Length {
            get { return End - Start; }
        }

        public bool Contains(int position) {
            if (!this.IsWellFormed && position == _end)
                return true;

            return TextRange.Contains(this, position);
        }

        public virtual void Shift(int offset) {
            _start += offset;
            _end += offset;
        }

        #endregion

        #region IExpandableTextRange
        public void Expand(int startOffset, int endOffset) {
            _start += startOffset;
            _end += endOffset;
        }

        public bool AllowZeroLength {
            get {
                return false;
            }
        }

        public bool IsStartInclusive {
            get {
                return true;
            }
        }

        public bool IsEndInclusive {
            get {
                return false;
            }
        }

        public bool ContainsUsingInclusion(int position) {
            return Contains(position);
        }
        #endregion
    }
}
