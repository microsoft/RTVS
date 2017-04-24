// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Outline;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.R.Editor.Outline {
    internal sealed class ROutliningTagger : OutliningTagger {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public ROutliningTagger(REditorDocument document, ICoreShell shell)
            : base(document.EditorTree.TextBuffer(), new ROutlineRegionBuilder(document, shell)) {
            document.Closing += OnDocumentClosing;
            document.EditorTree.EditorBuffer.AddService(this);
        }

        private void OnDocumentClosing(object sender, EventArgs e) {
            REditorDocument document = (REditorDocument)sender;
            document.Closing -= OnDocumentClosing;
            document.EditorTree.EditorBuffer.RemoveService(this);
        }

        public override OutliningRegionTag CreateTag(OutlineRegion region) => new ROutliningRegionTag(region);
    }
}