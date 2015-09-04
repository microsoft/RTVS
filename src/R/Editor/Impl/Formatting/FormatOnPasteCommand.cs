using System;
using System.Windows;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting
{
    public class FormatOnPasteCommand : EditingCommand
    {
        public FormatOnPasteCommand(ITextView textView, ITextBuffer textBuffer) :
            base(textView, new CommandId(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Paste))
        {
        }

        public override CommandStatus Status(Guid group, int id)
        {
            if (REditorSettings.FormatOnPaste &&
                (Clipboard.ContainsData(DataFormats.Text) || Clipboard.ContainsData(DataFormats.UnicodeText)))
            {
                return CommandStatus.SupportedAndEnabled;
            }

            return CommandStatus.NotSupported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg)
        {
            if (!REditorSettings.FormatOnPaste)
            {
                return CommandResult.NotSupported;
            }

            string text = Clipboard.GetData(DataFormats.UnicodeText) as string;
            if (text == null)
            {
                text = Clipboard.GetData(DataFormats.Text) as string;
            }

            if (text != null)
            {
                int insertionPoint = TextView.Caret.Position.BufferPosition.Position;
                TextView.TextBuffer.Replace(new Span(insertionPoint, 0), text);

                if (REditorSettings.FormatOnPaste)
                {
                    IREditorDocument document = EditorDocument.FromTextBuffer(TextView.TextBuffer);
                    RangeFormatter.FormatRange(TextView, new TextRange(insertionPoint, text.Length), document.EditorTree.AstRoot, REditorSettings.FormatOptions);
                }
            }

            return CommandResult.Executed;
        }
    }
}
