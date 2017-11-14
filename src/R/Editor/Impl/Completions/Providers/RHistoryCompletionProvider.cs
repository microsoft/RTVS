// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Imaging;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Components.History;

namespace Microsoft.R.Editor.Completions.Providers {
    internal sealed class RHistoryCompletionProvider : IRCompletionListProvider {
        private const char Ellipsis = '\u2026';

        private readonly IRHistory _history;
        private readonly object _glyph;
        public bool AllowSorting => true;

        public RHistoryCompletionProvider(IRHistory history, IImageService imageService) {
            _history = history;
            _glyph = imageService.GetImage("History");
        }

        public IReadOnlyCollection<ICompletionEntry> GetEntries(IRIntellisenseContext context, string prefixFilter = null) {
            var snapshot = context.EditorBuffer.CurrentSnapshot;
            var lineStart = snapshot.GetLineFromPosition(context.Position).Start;
            var searchText = snapshot.GetText(new TextRange(lineStart, context.Position - lineStart)).Trim();
            var entries = new List<ICompletionEntry>();
            foreach (var text in _history.Search(searchText)) {
                var displayText = GetDisplayText(text);
                var descriptionText = GetDescriptionText(text, displayText);
                entries.Add(new EditorCompletionEntry(displayText, text, descriptionText, _glyph));
            }

            return entries;
        }

        private static string GetDisplayText(string text) {
            const int displayTextMaxLength = 31;

            var firstLineEndIndex = text.IndexOfAny(CharExtensions.LineBreakChars);
            if (firstLineEndIndex >= 0 && firstLineEndIndex <= displayTextMaxLength) {
                return text.Substring(0, firstLineEndIndex) + Ellipsis;
            }

            return text.Length > displayTextMaxLength ? text.Substring(0, displayTextMaxLength - 1) + Ellipsis : text;
        }

        private static string GetDescriptionText(string text, string displayText) {
            const int descriptionTextMaxLength = 513;
            return text.Length > descriptionTextMaxLength 
                ? text.Substring(0, descriptionTextMaxLength - 1) + Ellipsis 
                : text.Length > displayText.Length ? text : string.Empty;
        }
    }
}