using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.IO;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.VisualStudio.R.Package.History {
    [Export(typeof(IRHistoryProvider))]
    internal class RHistoryProvider : IRHistoryProvider {
        [Import]
        private IEditorOperationsFactoryService EditorOperationsFactory { get; set; }

        [Import]
        private IRtfBuilderService RtfBuilderService { get; set; }

        [Import]
        private IFileSystem FileSystem { get; set; }

        public IRHistory GetAssociatedRHistory(ITextView textView) {
            return textView.Properties.GetOrCreateSingletonProperty(typeof(RHistory), () => CreateRHistory(textView));
        }

        private RHistory CreateRHistory(ITextView textView) {
            return new RHistory(textView, FileSystem, EditorOperationsFactory, RtfBuilderService, AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell)));
        }
    }
}