// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser {
    public sealed partial class HtmlParser {
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "doctype")]
        internal void OnStartTagState() {
            Debug.Assert(_cs.CurrentChar == '<');

            NameToken nameToken = null;
            int openAngleBracketPosition = _cs.Position;
            _cs.MoveToNextChar();

            bool isDocType = false;
            bool isXmlPi = false;
            bool isScript = false;
            bool isStyle = false;
            string scriptType = String.Empty;

            if (_cs.CurrentChar == '!') {
                if (_cs.Text.CompareTo(_cs.Position, 9, "!doctype ", true)) {
                    UpdateDocType();

                    isDocType = true;

                    nameToken = NameToken.Create(openAngleBracketPosition + 1, 8);
                    _cs.Position = openAngleBracketPosition + 9;
                }
            } else if (_cs.CurrentChar == '?') {
                if (_cs.Text.CompareTo(_cs.Position, 4, "?xml", true)) {
                    isXmlPi = true;
                    nameToken = NameToken.Create(openAngleBracketPosition + 1, 4);
                    _cs.Position = openAngleBracketPosition + 5;

                    ParsingMode = ParsingMode.Xml;
                }
            } else if (_cs.CurrentChar != '%') {
                // If element name is missing, do not parse this as a tag.
                // Example: a < b is just a text.
                nameToken = _tokenizer.GetNameToken();
            }

            if (nameToken == null)
                return;

            if (nameToken.HasName()) {
                string qualifiedName = _cs.GetSubstringAt(nameToken.Start, nameToken.Length);
                IReadOnlyList<string> scriptTagNames = ScriptOrStyleTagNameService.GetScriptTagNames();
                StringComparer comparer = (this.ParsingMode == ParsingMode.Html ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

                if (scriptTagNames.Contains(qualifiedName, comparer)) {
                    isScript = true;
                } else {
                    IReadOnlyList<string> styleTagNames = ScriptOrStyleTagNameService.GetStyleTagNames();
                    if (styleTagNames.Contains(qualifiedName, comparer)) {
                        isStyle = true;
                    }
                }
            }

            // We do not allow <% %> as an element name since although it is possible, 
            // it is pretty exotic. We do special case <!doctype> though.

            Debug.Assert(nameToken.Length > 0);

            if (StartTagOpen != null)
                StartTagOpen(this, new HtmlParserOpenTagEventArgs(openAngleBracketPosition, nameToken, isDocType, isXmlPi));

            while (true) {
                _tokenizer.SkipWhitespace();
                if (_cs.IsEndOfStream()) {
                    CloseStartTag(_cs.Position, 0, false, false);
                    break;
                }

                if (_cs.CurrentChar == '/') {
                    int length = _cs.NextChar == '>' ? 2 : 1;
                    CloseStartTag(_cs.Position, length, true, length > 1);
                    break;
                }

                if (isXmlPi && _cs.CurrentChar == '?' && _cs.NextChar == '>') {
                    CloseStartTag(_cs.Position, 2, selfClose: true, wellFormed: true);
                    break;
                }

                if (_cs.CurrentChar == '>') // <script> and <style> are not self-closing
                {
                    CloseStartTag(_cs.Position, 1, selfClose: false, wellFormed: true);

                    if (ParsingMode != ParsingMode.Xml) {
                        if (isScript) {
                            OnScriptState(scriptType, nameToken);
                        } else if (isStyle) {
                            OnStyleState(nameToken);
                        }
                    }

                    break;
                }

                if (_cs.CurrentChar == '<') {
                    // unterminated start tag
                    CloseStartTag(_cs.Position, 0, selfClose: false, wellFormed: false);
                    break;
                }

                var attributeToken = OnAttributeState(false);

                if (isScript && attributeToken != null && attributeToken.HasName() && attributeToken.HasValue()) {
                    var comparison = this.ParsingMode == ParsingMode.Html ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

                    var name = _cs.GetSubstringAt(attributeToken.NameToken.Start, attributeToken.NameToken.Length);
                    if (name.Equals("type", comparison)) {
                        scriptType = GetAttributeValue(attributeToken.ValueToken);

                        if (this.ScriptTypeResolution != null)
                            isScript = !this.ScriptTypeResolution.IsScriptContentMarkup(scriptType);
                    }
                }
            }
        }

        private string GetAttributeValue(IHtmlAttributeValueToken valueToken) {
            int start = valueToken.Start;
            if (valueToken.OpenQuote != '\0')
                start++;

            int end = valueToken.End;
            if (valueToken.Length > 1 && valueToken.CloseQuote != '\0')
                end--;

            if (end > start)
                return _cs.GetSubstringAt(start, end - start);

            return String.Empty;
        }

        private void CloseStartTag(int position, int length, bool selfClose, bool wellFormed) {
            var range = new TextRange(position, length);

            StartTagClose?.Invoke(this, new HtmlParserCloseTagEventArgs(range, wellFormed, selfClose));
            _cs.Advance(length);
        }

        private void UpdateDocType() {
            if (DocType != DocType.Undefined)
                return;

            // Simple handling since <!doctype> tag must be well formed
            // to be properly recognized as such.
            int startPosition = _cs.Position;

            while (!_cs.IsEndOfStream() && _cs.CurrentChar != '>' && _cs.CurrentChar != '<') {
                _cs.MoveToNextChar();
            }

            if (_cs.CurrentChar != '>')
                return;

            string docTypeText = Text.GetText(new TextRange(startPosition - 1, _cs.Position - startPosition + 2));
            DocType = DocTypeSignatures.GetDocType(docTypeText);

            UpdateParseMode(DocType);
        }

        void UpdateParseMode(DocType docType) {
            switch (docType) {
                case DocType.Xhtml10Transitional:
                case DocType.Xhtml10Strict:
                case DocType.Xhtml10Frameset:
                case DocType.Xhtml11:
                case DocType.Xhtml20:
                    ParsingMode = ParsingMode.Xhtml;
                    break;

                case DocType.Undefined:
                case DocType.Unrecognized:
                case DocType.Html32:
                case DocType.Html401Transitional:
                case DocType.Html401Strict:
                case DocType.Html401Frameset:
                case DocType.Html5:
                default:
                    ParsingMode = ParsingMode.Html;
                    break;
            }
        }
    }
}
