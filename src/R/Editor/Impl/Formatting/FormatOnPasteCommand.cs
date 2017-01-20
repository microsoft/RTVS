// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Controller;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Formatting.Data;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    public class FormatOnPasteCommand : EditingCommand {
        internal IClipboardDataProvider ClipboardDataProvider { get; set; }

        public FormatOnPasteCommand(ITextView textView, ITextBuffer textBuffer, IEditorShell editorShell) :
            base(textView, editorShell, new CommandId(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Paste)) {
            ClipboardDataProvider = new ClipboardDataProvider();
        }

        public override CommandStatus Status(Guid group, int id) {
            if (REditorSettings.FormatOnPaste &&
                (ClipboardDataProvider.ContainsData(DataFormats.Text) || ClipboardDataProvider.ContainsData(DataFormats.UnicodeText))) {
                return CommandStatus.SupportedAndEnabled;
            }
            return CommandStatus.NotSupported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (!REditorSettings.FormatOnPaste || TextView.Selection.Mode != TextSelectionMode.Stream) {
                return CommandResult.NotSupported;
            }

            string text = ClipboardDataProvider.GetData(DataFormats.UnicodeText) as string;
            if (text == null) {
                text = ClipboardDataProvider.GetData(DataFormats.Text) as string;
            }

            if (text != null) {
                var rSpans = TextView.BufferGraph.MapDownToFirstMatch(
                    TextView.Selection.StreamSelectionSpan.SnapshotSpan,
                    SpanTrackingMode.EdgeInclusive,
                    snapshot => snapshot.TextBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType)
                );
                if (rSpans.Count > 0) {
                    var targetSpan = rSpans[rSpans.Count - 1];

                    IREditorDocument document = REditorDocument.TryFromTextBuffer(targetSpan.Snapshot.TextBuffer);
                    if (document != null) {
                        int insertionPoint = targetSpan.Start;
                        document.TextBuffer.Replace(targetSpan, text);
                        document.EditorTree.EnsureTreeReady();

                        // We don't want to format inside strings
                        if (!document.EditorTree.AstRoot.IsPositionInsideString(insertionPoint)) {
                            RangeFormatter.FormatRange(TextView, document.TextBuffer,
                                new TextRange(insertionPoint, text.Length), REditorSettings.FormatOptions, EditorShell);
                        }
                    }
                }
            }
            return CommandResult.Executed;
        }
    }
}
