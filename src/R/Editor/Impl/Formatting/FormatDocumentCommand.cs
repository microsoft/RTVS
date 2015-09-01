using System;
using System.Diagnostics;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.Selection;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.Undo;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting
{
    internal class FormatDocumentCommand : EditingCommand
    {
        ITextBuffer _textBuffer;

        internal FormatDocumentCommand(ITextView textView, ITextBuffer textBuffer)
            : base(textView, new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.FORMATDOCUMENT))
        {
            _textBuffer = textBuffer;
        }

        #region ICommand
        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg)
        {
            string originalText = _textBuffer.CurrentSnapshot.GetText();
            string formattedText = string.Empty;
            var formatter = new RFormatter(REditorSettings.FormatOptions);

            try
            {
                formattedText = formatter.Format(originalText);
            }
            catch (Exception ex)
            {
                Debug.Assert(false, "Formatter exception: ", ex.Message);
            }

            if (!string.IsNullOrEmpty(formattedText) && !string.Equals(formattedText, originalText, StringComparison.Ordinal))
            {
                var selectionTracker = new RSelectionTracker(TextView, _textBuffer);
                selectionTracker.StartTracking(automaticTracking: false);

                try
                {
                    using (var massiveChange = new MassiveChange(TextView, _textBuffer, Resources.FormatDocument))
                    {
                        IREditorDocument document = EditorDocument.FromTextBuffer(_textBuffer);

                        document.EditorTree.Invalidate();

                        var caretPosition = TextView.Caret.Position.BufferPosition;
                        var viewPortLeft = TextView.ViewportLeft;

                        IncrementalTextChangeApplication.ApplyChange(_textBuffer, 0, _textBuffer.CurrentSnapshot.Length, formattedText,
                                                                     Resources.FormatDocument, selectionTracker, Int32.MaxValue);
                    }
                }
                finally
                {
                    selectionTracker.EndTracking();
                }

                return new CommandResult(CommandStatus.Supported, 0);
            }

            return CommandResult.NotSupported;
        }

        public override CommandStatus Status(Guid group, int id)
        {
            return CommandStatus.SupportedAndEnabled;
        }
        #endregion
    }
}
