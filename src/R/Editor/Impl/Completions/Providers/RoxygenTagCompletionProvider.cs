// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Imaging;
using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Editor.Roxygen;

namespace Microsoft.R.Editor.Completions.Providers {
    internal sealed class RoxygenTagCompletionProvider : IRCompletionListProvider {
        private readonly object _glyph;

        public bool AllowSorting => true;

        public RoxygenTagCompletionProvider(IImageService imageService) {
            _glyph = imageService.GetImage(ImageType.Keyword);
        }

        public IReadOnlyCollection<ICompletionEntry> GetEntries(IRIntellisenseContext context, string prefixFilter = null) {
            var completions = new List<ICompletionEntry>();

            var line = context.EditorBuffer.CurrentSnapshot.GetLineFromPosition(context.Position);
            var rawLineText = line.GetText();
            var lineText = rawLineText.TrimStart();
            var positionInLine = context.Position - line.Start;
            var typedText = lineText.TextBeforePosition(positionInLine);

            // Check that we are inside the Roxygen comment AND just typed character is @
            if (lineText.StartsWith("#'") && positionInLine >= 2 && typedText.StartsWithOrdinal("@")) {
                completions.AddRange(RoxygenKeywords.Keywords.Select(k => new EditorCompletionEntry(k, k, string.Empty, _glyph)));
            }
            return completions;
        }
    }
}
