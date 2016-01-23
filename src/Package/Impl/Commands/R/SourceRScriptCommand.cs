using System;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.R.Components.Controller;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Commands {
    public sealed class SourceRScriptCommand : ViewCommand {
        private static readonly CommandId[] Commands =  {
            new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSourceRScript)
        };

        private readonly IVsMonitorSelection _monitorSelection;
        private readonly uint _debugUIContextCookie;
        private readonly IRInteractiveSession _interactiveSession;

        public SourceRScriptCommand(ITextView textView, IRInteractiveSession interactiveSession)
            : base(textView, Commands, false) {
            _interactiveSession = interactiveSession;
            ReplWindow.EnsureReplWindow().DoNotWait();

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

        private string GetFilePath() {
            ITextDocument document;
            if (TextView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out document)) {
                return document.FilePath;
            }

            return null;
        }

        public override CommandStatus Status(Guid group, int id) {
            return GetFilePath() != null ? CommandStatus.SupportedAndEnabled : CommandStatus.NotSupported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            string filePath = GetFilePath();
            if (filePath == null) {
                return CommandResult.NotSupported;
            }

            // Save file before sourcing
            TextView.SaveFile();

            _interactiveSession.ExecuteExpression($"{(IsDebugging() ? "rtvs::debug_source" : "source")}({filePath.ToRStringLiteral()})");
            return CommandResult.Executed;
        }
    }
}
