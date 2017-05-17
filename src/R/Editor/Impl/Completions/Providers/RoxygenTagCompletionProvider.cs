// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core.Imaging;
using Microsoft.Languages.Editor.Completions;

namespace Microsoft.R.Editor.Completions.Providers {
    internal sealed class RoxygenTagCompletionProvider : IRCompletionListProvider {
        private readonly object _glyph;

        public bool AllowSorting => true;

        public RoxygenTagCompletionProvider(IImageService imageService) {
            _glyph = imageService.GetImage(ImageType.Keyword);
        }

        public IReadOnlyCollection<ICompletionEntry> GetEntries(IRIntellisenseContext context) {
            var completions = new List<ICompletionEntry>();

            var line = context.EditorBuffer.CurrentSnapshot.GetLineFromPosition(context.Position);
            var rawLineText = line.GetText();
            var lineText = rawLineText.TrimStart();

            // Check that we are inside the Roxygen comment
            if (!lineText.StartsWith("#'") || context.Position < rawLineText.Length - lineText.Length + 2) {
                return completions;
            }
            if (lineText[context.Position - line.Start - 1] != '@') {
                return completions;
            }

            completions.AddRange(_keywords.Select(k => new EditorCompletionEntry(k, k, string.Empty, _glyph)));
            return completions;
        }

        private static readonly string[] _keywords = {
            "@aliases",
            "@author",
            "@concents",
            "@describeIn",
            "@description",
            "@details",
            "@docType",
            "@evalRd",
            "@example",
            "@examples",
            "@export",
            "@exportClass",
            "@exportMethod",
            "@family",
            "@field",
            "@format",
            "@import",
            "@importClassesFrom",
            "@importFrom",
            "@importMethodsFrom",
            "@include",
            "@inheritDotParams",
            "@inheritParams",
            "@inheritSection",
            "@keywords",
            "@method",
            "@name",
            "@note",
            "@noRd",
            "@param",
            "@rawRd",
            "@rawNamespace",
            "@rdname",
            "@references",
            "@return",
            "@section",
            "@seealso",
            "@slot",
            "@source",
            "@template",
            "@templateVar",
            "@title",
            "@usage",
            "@useDynLib"
        };
    }
}
