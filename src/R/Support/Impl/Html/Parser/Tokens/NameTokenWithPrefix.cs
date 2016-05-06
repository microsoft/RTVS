// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser.Tokens {
    public class NameTokenWithPrefix : NameToken {
        private int _prefixStart;
        private int _prefixEnd;
        private int _colonPos;

        #region Constructors
        public NameTokenWithPrefix(int prefixStart, int prefixLength, int colonPos, int nameStart, int nameEnd)
            : base(nameStart, nameEnd) {
            _prefixStart = prefixStart;
            _prefixEnd = prefixStart + prefixLength;

            _colonPos = colonPos;
        }

        public NameTokenWithPrefix(TextRange prefixRange, int colonPos, TextRange nameRange) :
            this(prefixRange.Start, prefixRange.Length, colonPos, nameRange.Start, nameRange.Length) {
        }
        #endregion

        /// <summary>
        /// Range of the prefix (may be invalid range)
        /// </summary>
        public override ITextRange PrefixRange {
            get {
                if (_prefixStart >= 0 && _prefixStart < _prefixEnd)
                    return TextRange.FromBounds(_prefixStart, _prefixEnd);

                return TextRange.EmptyRange;
            }
        }

        /// <summary>
        /// Range of the colon symbol (may be an invalid range)
        /// </summary>
        public override ITextRange ColonRange {
            get {
                if (_colonPos >= 0)
                    return new TextRange(_colonPos, 1);

                return TextRange.EmptyRange;
            }
        }

        #region ITextRange

        public override void Shift(int offset) {
            if (_prefixStart < _prefixEnd) {
                _prefixStart += offset;
                _prefixEnd += offset;
            }

            if (_colonPos >= 0)
                _colonPos += offset;

            base.Shift(offset);
        }

        #endregion
    }

}
