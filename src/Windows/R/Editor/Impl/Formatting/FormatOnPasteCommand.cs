// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Formatting.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    public class FormatOnPasteCommand : EditingCommand {
        private readonly IREditorSettings _settings;

        internal IClipboardDataProvider ClipboardDataProvider { get; set; }

        public FormatOnPasteCommand(ITextView textView, ITextBuffer textBuffer, IServiceContainer services) :
            base(textView, services, new CommandId(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Paste)) {
            ClipboardDataProvider = new ClipboardDataProvider();
            _settings = services.GetService<IREditorSettings>();
        }

        public override CommandStatus Status(Guid group, int id) {
            if (_settings.FormatOnPaste &&
                (ClipboardDataProvider.ContainsData(DataFormats.Text) || ClipboardDataProvider.ContainsData(DataFormats.UnicodeText))) {
                return CommandStatus.SupportedAndEnabled;
            }
            return CommandStatus.NotSupported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (!_settings.FormatOnPaste || TextView.Selection.Mode != TextSelectionMode.Stream) {
                return CommandResult.NotSupported;
            }

            string text = ClipboardDataProvider.GetData(DataFormats.UnicodeText) as string ?? 
                          ClipboardDataProvider.GetData(DataFormats.Text) as string;

            if (text != null) {
                var rSpans = TextView.BufferGraph.MapDownToFirstMatch(
                    TextView.Selection.StreamSelectionSpan.SnapshotSpan,
                    SpanTrackingMode.EdgeInclusive,
                    snapshot => snapshot.TextBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType)
                );
                if (rSpans.Count > 0) {
                    var targetSpan = rSpans[rSpans.Count - 1];

                    var document = targetSpan.Snapshot.TextBuffer.GetEditorDocument<IREditorDocument>();
                    if (document != null) {
                        int insertionPoint = targetSpan.Start;
                        document.EditorBuffer.Replace(targetSpan.ToTextRange(), text);
                        document.EditorTree.EnsureTreeReady();

                        // We don't want to format inside strings
                        if (!document.EditorTree.AstRoot.IsPositionInsideString(insertionPoint)) {
                            RangeFormatter.FormatRange(TextView.ToEditorView(), document.EditorBuffer,
                                new TextRange(insertionPoint, text.Length), _settings, Services);
                        }
                    }
                }
            }
            return CommandResult.Executed;
        }
    }
}
