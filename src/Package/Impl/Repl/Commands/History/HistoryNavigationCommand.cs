// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    public sealed class HistoryNavigationCommand : ViewCommand {
        private readonly ICompletionBroker _completionBroker;
        private readonly IEditorOperationsFactoryService _editorFactory;
        private readonly IRHistory _history;
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;

        public HistoryNavigationCommand(ITextView textView, IRInteractiveWorkflowVisual interactiveWorkflow, ICompletionBroker completionBroker, IEditorOperationsFactoryService editorFactory) :
            base(textView, new[] {
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.UP),
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.DOWN),
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.LEFT),
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.RIGHT),
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.UP_EXT),
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.DOWN_EXT),
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.LEFT_EXT),
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.RIGHT_EXT)
            }, false) {
            _completionBroker = completionBroker;
            _editorFactory = editorFactory;
            _interactiveWorkflow = interactiveWorkflow;
            _history = interactiveWorkflow.History;
        }

        public override CommandStatus Status(Guid group, int id) {
            if (_completionBroker.IsCompletionActive(TextView)) {
                return CommandStatus.NotSupported;
            }

            return CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (_completionBroker.IsCompletionActive(TextView)) {
                return CommandResult.NotSupported;
            }
            var window = _interactiveWorkflow.ActiveWindow;
            var curPoint = window.TextView.MapDownToBuffer(
                window.TextView.Caret.Position.BufferPosition,
                window.CurrentLanguageBuffer
            );

            bool extend = id == (int)VSConstants.VSStd2KCmdID.UP_EXT ||
                        id == (int)VSConstants.VSStd2KCmdID.DOWN_EXT ||
                        id == (int)VSConstants.VSStd2KCmdID.RIGHT_EXT ||
                        id == (int)VSConstants.VSStd2KCmdID.LEFT_EXT;

            var editorOps = _editorFactory.GetEditorOperations(TextView);
            if (curPoint != null) {
                var curLine = curPoint.Value.GetContainingLine();
                switch ((VSConstants.VSStd2KCmdID)id) {
                    // history navigates if we're on the top or bottom line
                    case VSConstants.VSStd2KCmdID.UP:
                        if (curLine.LineNumber == 0) {
                            // this leaves the caret at the end which is what we want for up/down to work nicely
                            _history.PreviousEntry();
                            return CommandResult.Executed;
                        }
                        break;
                    case VSConstants.VSStd2KCmdID.DOWN:
                        if (curLine.LineNumber == curPoint.Value.Snapshot.LineCount - 1) {
                            _history.NextEntry();

                            // move the caret to the 1st line in history so down/up works nicely
                            var firstLine = window.CurrentLanguageBuffer.CurrentSnapshot.GetLineFromLineNumber(0);
                            var upperPoint = MapUp(window, firstLine.Start);
                            window.TextView.Caret.MoveTo(upperPoint.Value);
                            return CommandResult.Executed;
                        }
                        break;
                    case VSConstants.VSStd2KCmdID.LEFT:
                    case VSConstants.VSStd2KCmdID.LEFT_EXT:
                        if (curLine.Start == curPoint.Value) {
                            if (curLine.LineNumber != 0) {
                                // move to the end of the previous line rather then navigating into the prompts
                                editorOps.MoveLineUp(extend);
                                window.InteractiveWindow.Operations.End(extend);
                            }
                            return CommandResult.Executed;

                        }
                        break;
                    case VSConstants.VSStd2KCmdID.RIGHT:
                    case VSConstants.VSStd2KCmdID.RIGHT_EXT:
                        if (curLine.End == curPoint.Value) {
                            if (curLine.LineNumber != curPoint.Value.Snapshot.LineCount - 1) {
                                // move to the beginning of the next line rather then navigating into the prompts
                                editorOps.MoveLineDown(extend);

                                curPoint = window.TextView.MapDownToBuffer(
                                    window.TextView.Caret.Position.BufferPosition,
                                    window.CurrentLanguageBuffer
                                );
                                Debug.Assert(curPoint != null);
                                var start = curPoint.Value.GetContainingLine().Start;

                                // Home would be nice here, but it goes to the beginning of the first non-whitespace char
                                if (extend) {
                                    VirtualSnapshotPoint anchor = TextView.Selection.AnchorPoint;
                                    TextView.Caret.MoveTo(MapUp(window, start).Value);
                                    TextView.Selection.Select(anchor.TranslateTo(TextView.TextSnapshot), TextView.Caret.Position.VirtualBufferPosition);
                                } else {
                                    window.TextView.Caret.MoveTo(MapUp(window, start).Value);
                                }
                            }
                            return CommandResult.Executed;
                        }
                        break;
                }
            }

            // if we're anywhere outside of the current language buffer then let the normal
            // editor behavior continue.
            switch ((VSConstants.VSStd2KCmdID)id) {
                case VSConstants.VSStd2KCmdID.UP:
                case VSConstants.VSStd2KCmdID.UP_EXT:
                    editorOps.MoveLineUp(extend);
                    break;
                case VSConstants.VSStd2KCmdID.DOWN:
                case VSConstants.VSStd2KCmdID.DOWN_EXT:
                    editorOps.MoveLineDown(extend);
                    break;
                case VSConstants.VSStd2KCmdID.LEFT:
                case VSConstants.VSStd2KCmdID.LEFT_EXT:
                    editorOps.MoveToPreviousCharacter(extend);
                    break;
                case VSConstants.VSStd2KCmdID.RIGHT:
                case VSConstants.VSStd2KCmdID.RIGHT_EXT:
                    editorOps.MoveToNextCharacter(extend);
                    break;
            }

            return CommandResult.Executed;
        }

        private SnapshotPoint? MapUp(IInteractiveWindowVisualComponent window, SnapshotPoint point) {
            return window.TextView.BufferGraph.MapUpToBuffer(
                point,
                PointTrackingMode.Positive,
                PositionAffinity.Successor,
                TextView.TextBuffer
            );
        }
    }
}
