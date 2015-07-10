using System;
using Microsoft.Languages.Editor.Outline;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.R.Editor.Outline
{
    internal sealed class ROutliningTagger : OutliningTagger
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public ROutliningTagger(EditorDocument document)
            : base(document.EditorTree.TextBuffer, new ROutlineRegionBuilder(document))
        {
            document.OnDocumentClosing += OnDocumentClosing;

            ServiceManager.AddService<ROutliningTagger>(this, document.EditorTree.TextBuffer);
        }

        private void OnDocumentClosing(object sender, EventArgs e)
        {
            EditorDocument document = (EditorDocument)sender;
            document.OnDocumentClosing -= OnDocumentClosing;

            ServiceManager.RemoveService<ROutliningTagger>(document.EditorTree.TextBuffer);
        }

        public override OutliningRegionTag CreateTag(OutlineRegion region)
        {
            return new ROutliningRegionTag(region);
        }
    }
}