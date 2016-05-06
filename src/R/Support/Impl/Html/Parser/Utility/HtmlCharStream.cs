// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser.Utility {
    public sealed class HtmlCharStream : CharacterStream {

        #region Constructors
        public HtmlCharStream(ITextProvider textAdapter)
            : base(textAdapter) {
        }

        public HtmlCharStream(ITextProvider textAdapter, ITextRange range)
            : base(textAdapter, range) {
        }

        public HtmlCharStream(string text)
            : base(text) {
        }
        #endregion

        public bool IsAtTagDelimiter() {
            return (CurrentChar == '<' || CurrentChar == '>' || (CurrentChar == '/' && NextChar == '>'));
        }

        public static bool IsNameStartChar(char ch) {
            // http://www.w3.org/TR/REC-xml/#charsets includes : in the name but
            // http://www.w3.org/TR/xml-names/ excludes it since it conflicts with 
            // namespace prefixes so we will not be considering <: as a legal tag.
            //
            // NameStartChar ::= ":" | [A-Z] | "_" | [a-z] | 
            //                      [#xC0-#xD6] | [#xD8-#xF6] | [#xF8-#x2FF] | [#x370-#x37D] | 
            //                      [#x37F-#x1FFF] | [#x200C-#x200D] | [#x2070-#x218F] | [#x2C00-#x2FEF] | 
            //                      [#x3001-#xD7FF] | [#xF900-#xFDCF] | [#xFDF0-#xFFFD] | [#x10000-#xEFFFF]         

            if (IsAnsiLetter(ch))
                return true;

            if (ch == '_' /* || CurrentChar == ':' */)
                return true;

            if (ch < 'A')
                return false;

            if (ch >= 0xC0 && ch <= 0xD6)
                return true;

            // xD7, xF7, x37E are excluded
            if (ch == 0xD7 || ch == 0xF7 || ch == 0x37E)
                return false;

            if (ch >= 0xC0 && ch <= 0x2FF)
                return true;

            if (ch >= 0x370 && ch <= 0x1FFF)
                return true;

            if (ch >= 0x200C && ch <= 0x200D)
                return true;

            if (ch >= 0x2070 && ch <= 0x218F)
                return true;

            if (ch >= 0x2C00 && ch <= 0x2FEF)
                return true;

            if (ch >= 0x3001 && ch <= 0xD7FF)
                return true;

            if (ch >= 0xF900 && ch <= 0xFDCF)
                return true;

            if (ch >= 0xFDF0 && ch <= 0xFFFD)
                return true;

            if (ch >= 0x2C00 && ch <= 0x2FEF)
                return true;

            return false;
        }

        public bool IsNameChar() {
            return IsNameChar(CurrentChar);
        }

        public static bool IsNameChar(char ch) {
            // http://www.w3.org/TR/REC-xml/#charsets
            // NameChar ::= NameStartChar | "-" | "." | [0-9] | #xB7 | [#x0300-#x036F] | [#x203F-#x2040] 
            // http://www.w3.org/TR/xml-names/ excludes first name character so one can't have <___ tags\
            // http://www.w3.org/TR/REC-xml/#NT-Char also discourages using of Unicode compatibility chars

            if (ch == '.' || ch == '-' || IsDecimal(ch) || ch == 0xB7)
                return true;

            if (IsNameStartChar(ch))
                return true;

            if (ch >= 0x300 && ch <= 0x36F)
                return true;

            if (ch >= 0x203F && ch <= 0x2040)
                return true;

            return false;
        }
    }
}
