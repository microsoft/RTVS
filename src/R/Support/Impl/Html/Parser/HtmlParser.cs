// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Html.Core.Parser.Utility;
using Microsoft.Html.Core.Tree;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser {
    // Parsing mode: start with HTML and check doctype. If we see XML processing instruction <?xml ... ?>
    // assume XML rules (case-sensitive elements). If doctype is HTML5, process comments in per HTML5 standard.
    // If doctype is any kind of HTML, assume case-insensitive elements. If doctype is XHTML (including XHTM5)
    // assume case-sensitive elements and comments per SGML standard.
    public sealed partial class HtmlParser {
        /// <summary>
        /// Specifies if parser is in HTML, XML or XHTMLmode. XHTML is case-sensitive so parser won't match 
        /// &lt;html&gt; to &lt;/HTML&gt; In XML mode no special handling is provided for &lt;script> or 
        /// &lt;style> blocks or attributes.
        /// </summary>
        public ParsingMode ParsingMode { get; private set; }

        /// <summary>
        /// Indicates Document Type of the document (as defined in a &lt;!DOCTYPE ...> element
        /// </summary>
        public DocType DocType { get; set; }

        ///// <summary>
        ///// Specifies if parser should handle comments as SGML or as HTML5. In SGML comment begins with -- and 
        ///// ends with -- and must reside inside <! ... > block. In HTML5 Comment begins with &lt;!-- and ends with --&gt;
        ///// </summary>
        //public bool IsHtml5Comments { get; set; }

        /// <summary>
        /// Text the parser is using
        /// </summary>
        public ITextProvider Text { get { return _cs.Text; } }

        /// <summary>
        /// A service that helps parser to detemine if content of a &lt;script>
        /// block should be skipped over (normal behavior) or should parser
        /// continue parsing inside the block since block content is actually
        /// a markup, like in &lt;script type="text/x-handlebars-template">
        /// </summary>
        public IHtmlScriptTypeResolutionService ScriptTypeResolution { get; set; }

        /// <summary>
        ///  Provides tag names for script and style tags.
        /// </summary>
        public IHtmlScriptOrStyleTagNamesService ScriptOrStyleTagNameService { get; private set; }

        /// <summary>
        /// Parser statistics
        /// </summary>
        public HtmlParserStatistic Stats { get; private set; }

        // Internal for unit tests
        internal HtmlCharStream _cs = null;
        internal HtmlTokenizer _tokenizer;

        /// <summary>
        /// Stop parsing once you passed this position in TextState
        /// Allows partial parse to end in a consistent state (e.g.
        /// we always want to parse till the end of the start tag/end tag,
        /// end of comment, end of artifact, etc., we don't want to stop
        /// parsing in the middle of an attribute value)
        /// </summary>
        internal int _softRangeEnd;

        /// <summary>
        /// Creates HTML parser with HTML parsing mode.
        /// </summary>
        public HtmlParser()
            : this(ParsingMode.Html, null, null) {
        }

        /// <summary>
        /// Creates HTML parser.
        /// </summary>
        /// <param name="parsingMode">
        /// Parsing mode (HTML, XHTML or XML). HTML and XHTML differ in element
        /// and attribute name case-sensitity while XML mode treats &lt;script>
        /// and &lt;style elements as regular elements.
        /// </param>
        public HtmlParser(ParsingMode parsingMode)
            : this(parsingMode, null, null) {
        }

        /// <summary>
        /// Creates HTML parser
        /// </summary>
        /// <param name="parsingMode">
        /// Parsing mode (HTML, XHTML or XML). HTML and XHTML differ in element
        /// and attribute name case-sensitity while XML mode treats &lt;script>
        /// and &lt;style elements as regular elements.
        /// </param>
        /// <param name="scriptTypeResolution">
        /// A service that helps parser to detemine if content of a &lt;script>
        /// block should be skipped over (normal behavior) or should parser
        /// continue parsing inside the block since block content is actually
        /// a markup, like in &lt;script type="text/x-handlebars-template">.
        /// </param>
        public HtmlParser(ParsingMode parsingMode, IHtmlScriptTypeResolutionService scriptTypeResolution, IHtmlScriptOrStyleTagNamesService scriptOrStyleTagNameService) {
            ParsingMode = parsingMode;
            ScriptTypeResolution = scriptTypeResolution;
            ScriptOrStyleTagNameService = (scriptOrStyleTagNameService ?? new DefaultScriptOrStyleTagNameService());
            Stats = new HtmlParserStatistic();
        }

        /// <summary>
        /// Parse HTML from a string
        /// </summary>
        /// <param name="text">String to parse</param>
        public void Parse(string text) {
            TextStream ts = new TextStream(text);
            Parse(ts, null);
        }

        /// <summary>
        /// Parse text from a text provider
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        public void Parse(ITextProvider textProvider) {
            Parse(textProvider, TextRange.FromBounds(0, textProvider.Length));
        }

        /// <summary>
        /// Parse text from a text provider within a given range
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="range">Range to parse</param>
        public void Parse(ITextProvider textProvider, ITextRange range) {
            DateTime? timeStart = null;

            if (Stats.Enabled)
                timeStart = DateTime.UtcNow;

            if (ParsingStarting != null)
                ParsingStarting(this, new HtmlParserRangeEventArgs(range));

            DocType = DocType.Undefined;

            _cs = new HtmlCharStream(textProvider, range);
            _tokenizer = new HtmlTokenizer(_cs);
            _softRangeEnd = range.End;

            OnTextState();

            if (ParsingComplete != null)
                ParsingComplete(this, new HtmlParserRangeEventArgs(range));

            if (Stats.Enabled) {
                Stats.ParseTime = (DateTime.UtcNow - timeStart.Value);
                Stats.CharactersPerSecond = (int)(1000.0 * (double)_cs.Length / (double)Stats.ParseTime.TotalMilliseconds + 0.5);
            }
        }
    }

    #region Statistics
    public class HtmlParserStatistic {
        public TimeSpan ParseTime { get; internal set; }

        private int _runs = 0;
        private int _charactersPerSecond = 0;

        public HtmlParserStatistic() {
            Enabled = false;
            Reset();
        }

        public bool Enabled { get; set; }

        public int CharactersPerSecond {
            get {
                return _charactersPerSecond;
            }

            internal set {
                _runs++;
                _charactersPerSecond = value;

                if (_runs == 1) {
                    AverageCps = value;
                } else {
                    AverageCps = (int)(((double)AverageCps) * (double)(_runs - 1) / ((double)_runs) + ((double)value) / ((double)_runs) + 0.5);
                }
            }
        }

        public int AverageCps { get; protected set; }

        public void Reset() {
            _runs = 0;
            _charactersPerSecond = 0;
            AverageCps = 0;
        }
    }
    #endregion
}
