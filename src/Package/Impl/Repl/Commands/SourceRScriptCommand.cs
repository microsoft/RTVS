using System;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal sealed class SourceRScriptCommand : PackageCommand {
        private readonly IRInteractiveWorkflowOperations _operations;
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly IVsMonitorSelection _monitorSelection;
        private readonly uint _debugUIContextCookie;
        private readonly IRInteractiveWorkflow _interactiveWorkflow;

        public SourceRScriptCommand(IRInteractiveWorkflow interactiveWorkflow, IActiveWpfTextViewTracker activeTextViewTracker)
            : base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSourceRScript) {
            _interactiveWorkflow = interactiveWorkflow;
            _activeTextViewTracker = activeTextViewTracker;
            _operations = interactiveWorkflow.Operations;
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
            return _activeTextViewTracker.GetLastActiveTextView(RContentTypeDefinition.ContentType);
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
            Visible = _interactiveWorkflow.ActiveWindow != null && _interactiveWorkflow.ActiveWindow.Container.IsOnScreen;
            Enabled = GetFilePath() != null;
        }

        internal override void Handle() {
            string filePath = GetFilePath();
            if (filePath != null) {
                // Save file before sourcing
                ITextView textView = GetActiveTextView();
                textView.SaveFile();
                _operations.ExecuteExpression($"{(IsDebugging() ? "rtvs::debug_source" : "source")}({filePath.ToRStringLiteral()})");
            }
        }
    }
}
