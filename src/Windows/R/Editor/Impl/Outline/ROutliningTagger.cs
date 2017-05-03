// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Outline;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.R.Editor.Outline {
    internal sealed class ROutliningTagger : OutliningTagger {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public ROutliningTagger(IREditorDocument document, IServiceContainer services)
            : base(document.EditorTree.TextBuffer(), new ROutlineRegionBuilder(document, services)) {
            document.Closing += OnDocumentClosing;
        }

        private void OnDocumentClosing(object sender, EventArgs e) {
            var document = (IREditorDocument)sender;
            document.Closing -= OnDocumentClosing;
        }

        public override OutliningRegionTag CreateTag(OutlineRegion region) => new ROutliningRegionTag(region);
    }
}