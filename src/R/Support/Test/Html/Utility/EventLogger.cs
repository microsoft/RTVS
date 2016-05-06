// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Html.Core.Parser;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Test.Utility {
    [ExcludeFromCodeCoverage]
    internal sealed class EventLogger {
        public StringBuilder Log { get; private set; }
        HtmlParser _parser;

        public EventLogger(HtmlParser parser) {
            parser.AttributeFound += OnAttribute;
            parser.CommentFound += OnComment;
            parser.EndTagClose += OnEndTagClose;
            parser.EndTagOpen += OnEndTagOpen;
            parser.EntityFound += OnEntity;
            parser.ScriptBlockFound += OnScriptBlock;
            parser.StartTagClose += OnStartTagClose;
            parser.StartTagOpen += OnStartTagOpen;
            parser.StyleBlockFound += OnStyleBlock;

            Log = new StringBuilder();
            _parser = parser;
        }

        [DebuggerStepThrough]
        public override string ToString() {
            return Log.ToString();
        }

        void OnStartTagOpen(object sender, HtmlParserOpenTagEventArgs e) {
            var nameToken = e.NameToken;

            if (e.IsDocType) {
                parser_OnDoctypeTag(sender, e);
            } else {
                if (nameToken != null) {
                    ITextProvider text = _parser.Text;

                    Log.AppendFormat("OnStartTagOpen: {0}: @[{1} ... {2}] '{3}'\r\n\tHasNamespace: {4} @[{5} ... {6}]\r\n\tHasName: {7} @[{8} ... {9}]\r\n\r\n",
                                            TokenTypeAsString(e.NameToken.TokenType), e.NameToken.Start, e.NameToken.End, text.GetText(e.NameToken),
                                            nameToken.HasPrefix().ToString(), nameToken.PrefixRange.Start, nameToken.PrefixRange.End,
                                            nameToken.HasName().ToString(), nameToken.NameRange.Start, nameToken.NameRange.End);
                } else {
                    Log.Append("OnStartTagOpen: null\r\n");
                }
            }
        }

        void OnStartTagClose(object sender, HtmlParserCloseTagEventArgs e) {
            ITextProvider text = _parser.Text;

            Log.AppendFormat("OnStartTagClose: {0} ... {1} '{2}'\r\n\tIsClosed: {3}\r\n\tIsShortHand: {4}\r\n\r\n",
                    e.CloseAngleBracket.Start, e.CloseAngleBracket.End, text.GetText(e.CloseAngleBracket),
                    e.IsClosed, e.IsShorthand);
        }

        void OnStyleBlock(object sender, HtmlParserBlockRangeEventArgs e) {
            Log.AppendFormat("OnStyleBlock: {0} ... {1}\r\n\r\n", e.Range.Start, e.Range.End);
        }

        void OnScriptBlock(object sender, HtmlParserBlockRangeEventArgs e) {
            Log.AppendFormat("OnScriptBlock: {0} ... {1}\r\n\r\n", e.Range.Start, e.Range.End);
        }

        void OnEndTagOpen(object sender, HtmlParserOpenTagEventArgs e) {
            var nameToken = e.NameToken;

            if (nameToken != null) {
                ITextProvider text = _parser.Text;

                Log.AppendFormat("OnEndTagOpen: {0}: @[{1} ... {2}] '{3}'\r\n\tHasNamespace: {4} @[{5} ... {6}]\r\n\tHasName: {7} @[{8} ... {9}]\r\n\r\n",
                                        TokenTypeAsString(e.NameToken.TokenType), e.NameToken.Start, e.NameToken.End, text.GetText(e.NameToken),
                                        nameToken.HasPrefix().ToString(), nameToken.PrefixRange.Start, nameToken.PrefixRange.End,
                                        nameToken.HasName().ToString(), nameToken.NameRange.Start, nameToken.NameRange.End);
            } else {
                Log.Append("OnEndTagOpen: null\r\n");
            }
        }

        void OnEndTagClose(object sender, HtmlParserCloseTagEventArgs e) {
            ITextProvider text = _parser.Text;

            Log.AppendFormat("OnEndTagClose: {0} ... {1} '{2}'\r\n\tIsClosed: {3}\r\n\tIsShortHand: {4}\r\n\r\n",
                    e.CloseAngleBracket.Start, e.CloseAngleBracket.End, text.GetText(e.CloseAngleBracket),
                    e.IsClosed, e.IsShorthand);
        }

        void OnAttribute(object sender, HtmlParserAttributeEventArgs e) {
            var at = e.AttributeToken;
            ITextProvider text = _parser.Text;

            Debug.Assert(at.Start >= 0 && at.End >= 0);

            Log.AppendFormat("\tOnAttribute: @[{0} ... {1}] '{2}'\r\n\t\tHasName: {3}\r\n\t\tHasValue: {4}\r\n",
                                    at.Start, at.End, text.GetText(at), at.HasName(), at.HasValue());
            if (at.HasName()) {
                Log.AppendFormat("\t\tName: @[{0} ... {1}]\r\n", at.NameToken.Start, at.NameToken.End);

                var nameToken = at.NameToken as NameToken;
                if (nameToken != null) {
                    if (nameToken.HasPrefix()) {
                        Log.AppendFormat("\t\t\tNamespace: {0} @[{1} ... {2}] '{3}'\r\n",
                                           nameToken.HasPrefix(), nameToken.PrefixRange.Start, nameToken.PrefixRange.End,
                                           text.GetText(nameToken.PrefixRange));
                    } else {
                        Log.AppendFormat("\t\t\tNamespace: {0} @[{1} ... {2}]\r\n",
                                           nameToken.HasPrefix(), nameToken.PrefixRange.Start, nameToken.PrefixRange.End);
                    }

                    if (nameToken.HasName()) {
                        Log.AppendFormat("\t\t\tName: {0} @[{1} ... {2}] '{3}'\r\n",
                                            nameToken.HasName(), nameToken.NameRange.Start, nameToken.NameRange.End,
                                            text.GetText(nameToken.NameRange));
                    } else {
                        Log.AppendFormat("\t\t\tName: {0} @[{1} ... {2}]\r\n",
                                           nameToken.HasName(), nameToken.NameRange.Start, nameToken.NameRange.End);
                    }
                }
            }

            if (at.HasValue()) {
                Log.AppendFormat("\t\tValue: @[{0} ... {1}] '{2}'\r\n", at.ValueToken.Start, at.ValueToken.End,
                                        text.GetText(at.ValueToken));

                var tokens = at.ValueToken.Tokens;
                foreach (IHtmlToken t in tokens) {
                    AppendToken("\t\t\t", t);
                }
            }
            Log.Append("\r\n");
        }

        void OnEntity(object sender, HtmlParserRangeEventArgs e) {
            Log.AppendFormat("OnEntity: {0} ... {1}\r\n\r\n", e.Range.Start, e.Range.End);
        }

        void OnComment(object sender, HtmlParserCommentEventArgs e) {
            var c = e.CommentToken;
            var text = _parser.Text;

            Log.AppendFormat("OnComment: @[{0} ... {1}] '{2}'\r\n", c.Start, c.End, text.GetText(c));
            foreach (var t in c) {
                AppendToken("\t", t);
            }
            Log.Append("\r\n");
        }

        void AppendToken(string prefix, IHtmlToken t, bool isGhost = false) {
            if (t != null) {
                var text = _parser.Text;

                Debug.Assert(t.Start >= 0 && t.End >= 0);

                if (isGhost) {
                    Log.AppendFormat("{0}: {1}: @[{2} ... {3}] <<ghost>>\r\n\r\n", prefix,
                            TokenTypeAsString(t.TokenType), t.Start, t.End);
                } else {
                    Log.AppendFormat("{0}: {1}: @[{2} ... {3}] '{4}'\r\n\r\n", prefix,
                            TokenTypeAsString(t.TokenType), t.Start, t.End, text.GetText(t));
                }
            } else {
                Log.AppendFormat("{0} <<null>>\r\n\r\n", prefix);
            }
        }

        void parser_OnDoctypeTag(object sender, HtmlParserOpenTagEventArgs e) {
            var nameToken = e.NameToken;

            if (nameToken != null) {
                ITextProvider text = _parser.Text;

                Log.AppendFormat("OnDocType: {0}: @[{1} ... {2}] '{3}'\r\n\tHasNamespace: {4} @[{5} ... {6}]\r\n\tHasName: {7} @[{8} ... {9}]\r\n\r\n",
                                        TokenTypeAsString(e.NameToken.TokenType), e.NameToken.Start, e.NameToken.End, text.GetText(e.NameToken),
                                        nameToken.HasPrefix().ToString(), nameToken.PrefixRange.Start, nameToken.PrefixRange.End,
                                        nameToken.HasName().ToString(), nameToken.NameRange.Start, nameToken.NameRange.End);
            } else {
                Log.Append("OnDocType: null\r\n");
            }
        }



        string TokenTypeAsString(HtmlTokenType t) {
            string type = t.ToString();
            //int lastDot = type.LastIndexOf('.');
            return type;
        }
    }
}
