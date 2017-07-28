// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.SmartIndent {
    /// <summary>
    /// Provides block and smart indentation in R Markdown
    /// </summary>
    internal sealed class SmartIndent : ISmartIndent {
        private readonly ITextView _textView;
        private readonly ISmartIndent _smartIndent;

        public SmartIndent(ITextView textView, IServiceContainer services) {
            _textView = textView;
            // In markdown indent is default. In R block delegate to the R indenter.
            var locator = services.GetService<IContentTypeServiceLocator>();
            var sip = locator.GetService<ISmartIndentProvider>(RContentTypeDefinition.ContentType);
            _smartIndent = sip.CreateSmartIndent(_textView);
        }

        public int? GetDesiredIndentation(ITextSnapshotLine line) {
            // In markdown indent is default. In R block delegate to the R indenter.
            var point = _textView.MapDownToR(line.Start);
            if(point.HasValue) {
                return _smartIndent.GetDesiredIndentation(point.Value.Snapshot.GetLineFromPosition(point.Value));
            }
            return null;
        }

        public void Dispose() { }
    }
}
