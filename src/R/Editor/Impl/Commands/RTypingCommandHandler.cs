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
    internal class RTypingCommandHandler : TypingCommandHandler
    {
        public RTypingCommandHandler(ITextView textView)
            : base(textView)
        {
        }

        #region ICommand
        public override void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg)
        {
            if (group == VSConstants.VSStd2K)
            {
                char typedChar = GetTypedChar(group, id, inputArg);

                if (AutoFormat.IsAutoformatTriggerCharacter(typedChar)) {
                    IREditorDocument document = REditorDocument.TryFromTextBuffer(TextView.TextBuffer);
                    if (document != null) {
                        IEditorTree tree = document.EditorTree;
                        tree.EnsureTreeReady();
                        var rPoint = TextView.BufferGraph.MapDownToFirstMatch(
                            TextView.Caret.Position.BufferPosition,
                            PointTrackingMode.Positive,
                            snapshot => snapshot.TextBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType),
                            PositionAffinity.Successor
                        );
                        if (rPoint != null) {
                            AutoFormat.HandleAutoFormat(TextView, rPoint.Value.Snapshot.TextBuffer, tree.AstRoot, typedChar);
                        }
                    }
                }

                base.PostProcessInvoke(result, group, id, inputArg, ref outputArg);
            }
        }
        #endregion

        protected override CompletionController CompletionController
        {
            get { return ServiceManager.GetService<RCompletionController>(TextView); }
        }
    }
}
