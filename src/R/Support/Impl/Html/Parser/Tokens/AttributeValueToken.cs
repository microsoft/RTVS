// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser.Tokens {
    public class AttributeValueToken : BaseHtmlToken, IHtmlAttributeValueToken {
        public override HtmlTokenType TokenType => HtmlTokenType.String;
        public override bool IsWellFormed => true;

        public static AttributeValueToken Create(IHtmlToken token, char openQuote, char closeQuote) {
            if ((openQuote != '"') || (closeQuote != '"')) {
                return new ComplexAttributeValueToken(token, openQuote, closeQuote);
            }

            return new AttributeValueToken(token);
        }

        protected AttributeValueToken(IHtmlToken token)
            : base(token.Start, token.Length) {
        }

        #region IHtmlAttributeValueToken

        /// <summary>
        /// Opening quote characher or null character if attribute value is not quoted.
        /// </summary>
        public virtual char OpenQuote => '"';

        /// <summary>
        /// Closing quote characher or null character if attribute value has no closing quote.
        /// </summary>
        public virtual char CloseQuote => '"';

        /// <summary>
        /// True of attribute value is client script.
        /// </summary>
        public virtual bool IsScript => false;

        public virtual ReadOnlyTextRangeCollection<IHtmlToken> Tokens {
            get {
                return new ReadOnlyTextRangeCollection<IHtmlToken>(
                    new TextRangeCollection<IHtmlToken>(
                        new IHtmlToken[1] { new HtmlToken(this.TokenType, this.Start, this.Length) }));
            }
        }
        #endregion

        #region ITextRange Members

        public override int End {
            get {
                int end = base.End;

                if (end < 0)
                    end = Start + 1;

                Debug.Assert(end >= 0);
                return end;
            }
        }
        #endregion
    }
}
