using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Microsoft.Common.Core;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal sealed class SourceRScriptCommand : PackageCommand {
        [Import]
        private IActiveWpfTextViewTracker TextViewTracker { get; set; }

        [Import]
        private IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        private readonly ReplWindow _replWindow;
        private readonly IVsMonitorSelection _monitorSelection;
        private readonly uint _debugUIContextCookie;

        public SourceRScriptCommand(ICompositionService cs)
            : base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSourceRScript) {
           cs.SatisfyImportsOnce(this);

            ReplWindow.EnsureReplWindow().DoNotWait();
            _replWindow = ReplWindow.Current;

            _monitorSelection = VsAppShell.Current.GetGlobalService<IVsMonitorSelection>(typeof(SVsShellMonitorSelection));
            if (_monitorSelection != null) {
                var debugUIContextGuid = new Guid(UIContextGuids.Debugging);
                if (ErrorHandler.Failed(_monitorSelection.GetCmdUIContextCookie(ref debugUIContextGuid, out _debugUIContextCookie))) {
                    _monitorSelection = null;
                }
            }
        }

        private bool IsDebugging() {
            if (_monitorSelection == null) {
                return false;
            }

            int fActive;
            if (ErrorHandler.Succeeded(_monitorSelection.IsCmdUIContextActive(_debugUIContextCookie, out fActive))) {
                return fActive != 0;
            }

            return false;
        }

        private ITextView GetActiveTextView() {
            IContentType contentType = ContentTypeRegistryService.GetContentType(RContentTypeDefinition.ContentType);
            return TextViewTracker.GetLastActiveTextView(contentType);
        }

        private string GetFilePath() {
            ITextView textView = GetActiveTextView();
            if (textView != null && !textView.IsClosed) {
                ITextDocument document;
                if (textView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out document)) {
                    return document.FilePath;
                }
            }
            return null;
        }

        internal override void SetStatus() {
            Enabled = GetFilePath() != null;
        }

        internal override void Handle() {
            string filePath = GetFilePath();
            if (filePath != null) {
                // Save file before sourcing
                ITextView textView = GetActiveTextView();
                textView.SaveFile();
                _replWindow.ExecuteCode($"{(IsDebugging() ? "rtvs::debug_source" : "source")}({filePath.ToRStringLiteral()})");
            }
        }
    }
}
