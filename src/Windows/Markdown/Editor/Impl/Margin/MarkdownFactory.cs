// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Markdig;
using Markdig.Extensions.Footers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Margin {
    // Based on https://github.com/madskristensen/MarkdownEditor/blob/master/src/Parsing/MarkdownFactory.cs
    public static class MarkdownFactory {
        private const string AttachedExceptionKey = "attached-exception";
        private static readonly ConditionalWeakTable<ITextSnapshot, MarkdownDocument> CachedDocuments = new ConditionalWeakTable<ITextSnapshot, MarkdownDocument>();

        private static readonly object _syncRoot = new object();
        private static IServiceContainer _services;

        static MarkdownFactory() {
            Pipeline = new MarkdownPipelineBuilder()
                .UsePragmaLines()
                .Build();
        }

        public static MarkdownPipeline Pipeline { get; }

        public static Exception GetAttachedException(this MarkdownDocument markdownDocument)
            => markdownDocument.GetData(AttachedExceptionKey) as Exception;

        public static void Initialize(IServiceContainer services) => _services = services;

        public static MarkdownDocument ParseToMarkdown(this ITextSnapshot snapshot, string file = null) {
            lock (_syncRoot) {
                return CachedDocuments.GetValue(snapshot, key => {
                    var text = key.GetText();
                    var markdownDocument = ParseToMarkdown(text);
                    Parsed?.Invoke(snapshot, new ParsingEventArgs(markdownDocument, file, snapshot));
                    return markdownDocument;
                });
            }
        }

        public static MarkdownDocument ParseToMarkdown(string text, MarkdownPipeline pipeline = null) {
            // Safe version that will always return a MarkdownDocument even if there is an exception while parsing
            MarkdownDocument doc;
            pipeline = pipeline ?? Pipeline;

            // Try first to parse a document with all exceptions active
            try {
                doc = Markdig.Markdown.Parse(text, pipeline);
            } catch (Exception ex) {
                // If we have an error, remember it
                doc = new MarkdownDocument { Span = new SourceSpan(0, text.Length - 1) };
                // we attach the exception to the document that will be later displayed to the user
                doc.SetData(AttachedExceptionKey, ex);
            }
            return doc;
        }

        public static event EventHandler<ParsingEventArgs> Parsed;

        private static bool IsUrlValid(string file, string url) {
            if (string.IsNullOrWhiteSpace(url)) {
                return true;
            }
            if (url.Contains("://") || url.StartsWith("/") || url.StartsWith("#") || url.StartsWith("mailto:")) {
                return true;
            }
            if (url.Contains('\\')) {
                return false;
            }

            var query = url.IndexOf('?');
            if (query > -1) {
                url = url.Substring(0, query);
            }

            var fragment = url.IndexOf('#');
            if (fragment > -1) {
                url = url.Substring(0, fragment);
            }

            try {
                var decodedUrl = Uri.UnescapeDataString(url);
                var currentDir = Path.GetDirectoryName(file);
                if (currentDir != null) {
                    var path = Path.Combine(currentDir, decodedUrl);
                    var fs = _services.FileSystem();

                    if (fs.FileExists(path)) {
                        return true;
                    }

                    if (string.IsNullOrEmpty(Path.GetExtension(path)) &&
                        fs.FileExists(Path.ChangeExtension(path, MdContentTypeDefinition.FileExtension))) {
                        return true;
                    }
                }
                return false;
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                return true;
            }
        }

        public static bool MatchSmartBlock(string text) {
            MarkdownDocument doc;
            return MatchSmartBlock(text, out doc);
        }

        public static bool MatchSmartBlock(string text, out MarkdownDocument doc) {
            // Temp workaround for list items: We trim the beginning of the line to be able to parse nested list items.
            text = text.TrimStart();
            doc = ParseToMarkdown(text);
            return doc.Count != 0 && (doc[0] is QuoteBlock || doc[0] is ListBlock || (doc[0] is CodeBlock && !(doc[0] is FencedCodeBlock)) || doc[0] is FooterBlock);
        }

        public static bool TryParsePendingSmartBlock(this ITextView view, bool fullLine, out List<Block> blocks, out MarkdownDocument doc, out bool isEmptyLineText, out bool isEmptyLineAfterCaret) {
            // Pre-match the current line to detect a smart block
            var caretPosition = view.Caret.Position.BufferPosition.Position;
            var startLinePosition = view.Caret.ContainingTextViewLine.Start.Position;
            var endLinePosition = fullLine
                ? view.Caret.ContainingTextViewLine.EndIncludingLineBreak.Position
                : caretPosition;
            var text = view.TextBuffer.CurrentSnapshot.GetText(startLinePosition, endLinePosition - startLinePosition);
            blocks = null;
            doc = null;
            isEmptyLineText = false;
            isEmptyLineAfterCaret = false;

            MarkdownDocument singleLineDoc;
            if (!MatchSmartBlock(text, out singleLineDoc)) {
                return false;
            }

            // Detect any literal text inside the line we parsed
            // Detect any non-whitespace chars after the caret to the end of the line
            var textLine = view.TextBuffer.CurrentSnapshot.GetText(caretPosition, view.Caret.ContainingTextViewLine.End.Position - caretPosition);
            isEmptyLineAfterCaret = true;
            foreach (var t in textLine) {
                if (!char.IsWhiteSpace(t)) {
                    isEmptyLineAfterCaret = false;
                    break;
                }
            }

            // If there are any inlines in the single line, check if they are any non-empty literal
            isEmptyLineText = isEmptyLineAfterCaret;
            foreach (var inline in singleLineDoc.Descendants().OfType<LeafInline>()) {
                var literal = inline as LiteralInline;
                if (literal == null || !literal.Content.IsEmptyOrWhitespace()) {
                    isEmptyLineText = false;
                    break;
                }
            }

            // Parse only until the end of the line after the caret
            // Because most of the time, a user would have typed characters before typing return
            // it is not efficient to re-use cached MarkdownDocument from MarkdownFactory, as it may be invalid,
            // and then after the hit return, the browser would have to be updated with a new snapshot
            var snapshot = view.TextBuffer.CurrentSnapshot;
            var textFromTop = snapshot.GetText(0, endLinePosition);
            doc = ParseToMarkdown(textFromTop);
            var lastChild = doc.FindBlockAtPosition(caretPosition);
            if (lastChild == null || !lastChild.ContainsPosition(caretPosition)) {
                return false;
            }

            // Re-build list of blocks
            blocks = new List<Block>();
            var block = lastChild;
            while (block != null) {
                // We don't add ListBlock (as they should have list items)
                if (block != doc && !(block is ListBlock)) {
                    blocks.Add(block);
                }
                block = block.Parent;
            }
            blocks.Reverse();

            return blocks.Count > 0;
        }
    }
}
