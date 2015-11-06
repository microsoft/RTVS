using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Completion;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Editor.Completion;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.Formatting;
using Microsoft.R.Editor.Tree.Definitions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Commands {
    /// <summary>
    /// Processes typing in the R editor document. 
    /// Implements <seealso cref="ICommandTarget" /> 
    /// to receive typing as commands
    /// </summary>
    internal class RTypingCommandHandler : TypingCommandHandler {
        public RTypingCommandHandler(ITextView textView)
            : base(textView) {
        }

        #region ICommand
        public override void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg) {
            if (group == VSConstants.VSStd2K) {
                char typedChar = GetTypedChar(group, id, inputArg);
                if (AutoFormat.IsAutoformatTriggerCharacter(typedChar)) {
                    HandleAutoformat(typedChar);
                }

                base.PostProcessInvoke(result, group, id, inputArg, ref outputArg);
            }
        }
        #endregion

        protected override CompletionController CompletionController {
            get { return ServiceManager.GetService<RCompletionController>(TextView); }
        }

        private void HandleAutoformat(char typedChar) {
            IEditorTree tree;
            SnapshotPoint? rPoint = GetCaretPointInBuffer(out tree);
            if (rPoint.HasValue) {
                ITextBuffer subjectBuffer = rPoint.Value.Snapshot.TextBuffer;
                if (typedChar == '\r' || typedChar == '\n' || typedChar == ';') {
                    int offset = typedChar == '\r' || typedChar == '\n' ? -1 : 0;
                    AutoFormat.FormatLine(TextView, subjectBuffer, tree.AstRoot, offset);
                }
                else if(typedChar == '}') {
                    AutoFormat.FormatCurrentScope(TextView, subjectBuffer, tree.AstRoot, indentCaret: false);
                }
            }
        }

        private SnapshotPoint? GetCaretPointInBuffer(out IEditorTree tree) {
            tree = null;
            IREditorDocument document = REditorDocument.TryFromTextBuffer(TextView.TextBuffer);
            if (document != null) {
                tree = document.EditorTree;
                tree.EnsureTreeReady();
                return TextView.BufferGraph.MapDownToFirstMatch(
                    TextView.Caret.Position.BufferPosition,
                    PointTrackingMode.Positive,
                    snapshot => snapshot.TextBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType),
                    PositionAffinity.Successor
                );
            }

            return null;
        }
    }
}
