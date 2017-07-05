// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Controllers.Commands;
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

            var text = ClipboardDataProvider.GetData(DataFormats.UnicodeText) as string ??
                          ClipboardDataProvider.GetData(DataFormats.Text) as string;

            if (string.IsNullOrEmpty(text)) {
                return CommandResult.NotSupported;
            }

            var viewSpan = TextView.Selection.StreamSelectionSpan.SnapshotSpan;
            // Locate non-readonly R buffer. In REPL previous entries
            // are read-only and only prompt line is writeable.
            var insertionPoint = TextView.BufferGraph.MapDownToInsertionPoint(viewSpan.Start, PointTrackingMode.Positive, 
                s => s.TextBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType) &&
                s.TextBuffer.GetReadOnlyExtents(new Span(0, s.Length)).Count == 0);

            if (!insertionPoint.HasValue) {
                return CommandResult.NotSupported; // Proceed with default action.
            }

            var targetBuffer = insertionPoint.Value.Snapshot.TextBuffer;
            // In REPL target buffer may be just "text", such as when readline() is called.
            // We do not format non-R code content. Proceed with default action.
            if (!targetBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType)) {
                return CommandResult.NotSupported;
            }

            var document = targetBuffer.GetEditorDocument<IREditorDocument>();
            if (document == null) {
                return CommandResult.NotSupported;
            }

            // Make sure change actually did happen
            var editorBuffer = document.EditorBuffer;
            if (editorBuffer.Replace(new TextRange(insertionPoint.Value, viewSpan.Length), text)) {
                // Make sure AST is up to date so we can determine if position is inside a string.
                // Since we don't want to format inside strings.
                document.EditorTree.EnsureTreeReady();
                if (!document.EditorTree.AstRoot.IsPositionInsideString(insertionPoint.Value)) {
                    var formatter = new RangeFormatter(Services);
                    formatter.FormatRange(TextView.ToEditorView(), editorBuffer,
                        new TextRange(insertionPoint.Value, text.Length));
                }
                return CommandResult.Executed;
            }
            return CommandResult.NotSupported;
        }
    }
}
