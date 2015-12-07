using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.IO;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.VisualStudio.R.Package.History {
    [Export(typeof(IRHistoryProvider))]
    internal class RHistoryProvider : IRHistoryProvider {
        private const string IntraTextAdornmentBufferKey = "IntraTextAdornmentBuffer";

        [Import]
        private IEditorOperationsFactoryService EditorOperationsFactory { get; set; }

        [Import]
        private IRtfBuilderService RtfBuilderService { get; set; }

        [Import]
        private IFileSystem FileSystem { get; set; }

        [Import]
        private ITextSearchService2 TextSearchService { get; set; }

        public IRHistory GetAssociatedRHistory(ITextView textView) {
            return textView.Properties.GetOrCreateSingletonProperty(typeof(RHistory), () => CreateRHistory(textView));
        }

        private RHistory CreateRHistory(ITextView textView) {
            IElisionBuffer elisionBuffer;
            if (!textView.TextViewModel.Properties.TryGetProperty(IntraTextAdornmentBufferKey, out elisionBuffer)) {
                throw new InvalidOperationException("TextView should have PredefinedTextViewRoles.Structured view role");
            }

            var vsUiShell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            return new RHistory(textView, FileSystem, EditorOperationsFactory, elisionBuffer, RtfBuilderService, TextSearchService, vsUiShell);
        }
    }
}