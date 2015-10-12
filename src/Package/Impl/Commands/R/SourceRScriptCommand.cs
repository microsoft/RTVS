using System;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.R.Debugger;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Commands {
    public sealed class SourceRScriptCommand : ViewCommand {
        private static readonly CommandId[] _commands =  {
            new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSourceRScript)
        };

        private ReplWindow _replWindow;

        public SourceRScriptCommand(ITextView textView)
            : base(textView, _commands, false) {
            ReplWindow.EnsureReplWindow().DoNotWait();
            _replWindow = ReplWindow.Current;
        }

        private string GetFilePath() {
            ITextDocument document;
            if (TextView.TextBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out document)) {
                return document.FilePath;
            } else {
                return null;
            }
        }

        public override CommandStatus Status(Guid group, int id) {
            return GetFilePath() != null ? CommandStatus.SupportedAndEnabled : CommandStatus.NotSupported;
        }

        public unsafe override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            string filePath = GetFilePath();
            if (filePath == null) {
                return CommandResult.NotSupported;
            }

            _replWindow.ExecuteCode($"source({filePath.ToRStringLiteral()})");
            return CommandResult.Executed;
        }

        //protected override void Dispose(bool disposing) {
        //    if (_replWindow != null) {
        //        _replWindow.Dispose();
        //        _replWindow = null;
        //    }

        //    base.Dispose(disposing);
        //}
    }
}
