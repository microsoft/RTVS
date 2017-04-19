// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Navigation.Text {
    /// <summary>
    /// Implements R-language specific word selection, otherwise it delegates to the default 
    /// plain text structure navigator. Default VS structure navigator considers a.b.c to be
    /// 3 words while in R it is a single identifier. Text structure navigator provides
    /// word selection to find (Ctrl+F and Ctrl+F3) as well as to double-click.
    /// </summary>
    internal sealed class TextStructureNavigator : ITextStructureNavigator {
        private readonly ITextStructureNavigator _plainTextNavigator;
        private readonly ITextBuffer _textBuffer;

        public TextStructureNavigator(ITextBuffer textBuffer, IContentTypeRegistryService crs, ITextStructureNavigatorSelectorService nss) {
            ContentType = crs.GetContentType(RContentTypeDefinition.ContentType);
            _textBuffer = textBuffer;
            _plainTextNavigator = nss.CreateTextStructureNavigator(textBuffer, crs.GetContentType("text"));
        }

        public IContentType ContentType { get; }

        public TextExtent GetExtentOfWord(SnapshotPoint currentPosition) {
            SnapshotPoint? point = _textBuffer.MapDown(currentPosition, RContentTypeDefinition.ContentType);
            if (point.HasValue) {
                Span? span = RTextStructure.GetWordSpan(point.Value.Snapshot, point.Value.Position);
                if (span.HasValue && span.Value.Length > 0) {
                    var snapshotSpan = new SnapshotSpan(point.Value.Snapshot, span.Value);
                    var viewSpan = _textBuffer.MapUp(snapshotSpan, RContentTypeDefinition.ContentType);
                    if (viewSpan.HasValue) {
                        return new TextExtent(viewSpan.Value, isSignificant: true);
                    }
                }
            }
            return _plainTextNavigator.GetExtentOfWord(currentPosition);
        }

        public SnapshotSpan GetSpanOfEnclosing(SnapshotSpan activeSpan) {
            return _plainTextNavigator.GetSpanOfEnclosing(activeSpan);
        }

        public SnapshotSpan GetSpanOfFirstChild(SnapshotSpan activeSpan) {
            return _plainTextNavigator.GetSpanOfFirstChild(activeSpan);
        }

        public SnapshotSpan GetSpanOfNextSibling(SnapshotSpan activeSpan) {
            return _plainTextNavigator.GetSpanOfNextSibling(activeSpan);
        }

        public SnapshotSpan GetSpanOfPreviousSibling(SnapshotSpan activeSpan) {
            return _plainTextNavigator.GetSpanOfPreviousSibling(activeSpan);
        }
    }
}
