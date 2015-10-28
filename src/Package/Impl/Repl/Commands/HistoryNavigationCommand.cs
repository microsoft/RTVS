using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    public sealed class HistoryNavigationCommand : ViewCommand {
        [Import]
        private ICompletionBroker _completionBroker { get; set; }
        [Import]
        private IEditorOperationsFactoryService _editorFactory { get; set; }

        public HistoryNavigationCommand(ITextView textView) :
            base(textView, new CommandId[] {
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.UP),
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.DOWN),
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.LEFT),
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.RIGHT),
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.UP_EXT),
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.DOWN_EXT),
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.LEFT_EXT),
                new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.RIGHT_EXT)
            }, false) {
            EditorShell.Current.CompositionService.SatisfyImportsOnce(this);
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
            var window = ReplWindow.Current.GetInteractiveWindow().InteractiveWindow;
            var curPoint = window.TextView.MapDownToBuffer(
                window.TextView.Caret.Position.BufferPosition,
                window.CurrentLanguageBuffer
            );

            bool extend = id == (int)VSConstants.VSStd2KCmdID.UP_EXT ||
                        id == (int)VSConstants.VSStd2KCmdID.DOWN_EXT ||
                        id == (int)VSConstants.VSStd2KCmdID.RIGHT_EXT ||
                        id == (int)VSConstants.VSStd2KCmdID.LEFT_EXT;

            if (curPoint != null) {
                // history navigates if we're on the top or bottom line
                if (id == (int)VSConstants.VSStd2KCmdID.UP) {
                    if (curPoint.Value.GetContainingLine().LineNumber == 0) {
                        // this leaves the caret at the end which is what we want for up/down to work nicely
                        window.Operations.HistoryPrevious();
                        return CommandResult.Executed;
                    }
                } else if (id == (int)VSConstants.VSStd2KCmdID.DOWN) {
                    if (curPoint.Value.GetContainingLine().LineNumber == curPoint.Value.Snapshot.LineCount - 1) {
                        window.Operations.HistoryNext();

                        // move the caret to the 1st line in history so down/up works nicely
                        var firstLine = window.CurrentLanguageBuffer.CurrentSnapshot.GetLineFromLineNumber(0);
                        var upperPoint = MapUp(window, firstLine.Start);
                        window.TextView.Caret.MoveTo(upperPoint.Value);
                        return CommandResult.Executed;
                    }
                } else if (id == (int)VSConstants.VSStd2KCmdID.LEFT || id == (int)VSConstants.VSStd2KCmdID.LEFT_EXT) {
                    if (curPoint.Value.GetContainingLine().Start == curPoint.Value) {
                        if (curPoint.Value.GetContainingLine().LineNumber != 0) {
                            // move to the end of the previous line rather then navigating into the prompts
                            _editorFactory.GetEditorOperations(TextView).MoveLineUp(extend);
                            window.Operations.End(extend);
                        }
                        return CommandResult.Executed;

                    }
                } else if (id == (int)VSConstants.VSStd2KCmdID.RIGHT || id == (int)VSConstants.VSStd2KCmdID.RIGHT_EXT) {
                    if (curPoint.Value.GetContainingLine().End == curPoint.Value) {
                        if (curPoint.Value.GetContainingLine().LineNumber != curPoint.Value.Snapshot.LineCount - 1) {
                            // move to the beginning of the next line rather then navigating into the prompts
                            _editorFactory.GetEditorOperations(TextView).MoveLineDown(extend);

                            curPoint = window.TextView.MapDownToBuffer(
                                window.TextView.Caret.Position.BufferPosition,
                                window.CurrentLanguageBuffer
                            );
                            Debug.Assert(curPoint != null);
                            var start = curPoint.Value.GetContainingLine().Start;
                            
                            // Home would be nice here, but it goes to the beginning of the first non-whitespace char
                            if (extend) {
                                Text.VirtualSnapshotPoint anchor = TextView.Selection.AnchorPoint;
                                TextView.Caret.MoveTo(MapUp(window, start).Value);
                                TextView.Selection.Select(anchor.TranslateTo(TextView.TextSnapshot), TextView.Caret.Position.VirtualBufferPosition);
                            } else {
                                window.TextView.Caret.MoveTo(MapUp(window, start).Value);
                            }
                        }
                        return CommandResult.Executed;
                    }
                }
            }

            // if we're anywhere outside of the current language buffer then let the normal
            // editor behavior continue.
            switch ((VSConstants.VSStd2KCmdID)id) {
                case VSConstants.VSStd2KCmdID.UP:
                case VSConstants.VSStd2KCmdID.UP_EXT:
                    _editorFactory.GetEditorOperations(TextView).MoveLineUp(extend);
                    break;
                case VSConstants.VSStd2KCmdID.DOWN:
                case VSConstants.VSStd2KCmdID.DOWN_EXT:
                    _editorFactory.GetEditorOperations(TextView).MoveLineDown(extend);
                    break;
                case VSConstants.VSStd2KCmdID.LEFT:
                case VSConstants.VSStd2KCmdID.LEFT_EXT:
                    _editorFactory.GetEditorOperations(TextView).MoveToPreviousCharacter(extend);
                    break;
                case VSConstants.VSStd2KCmdID.RIGHT:
                case VSConstants.VSStd2KCmdID.RIGHT_EXT:
                    _editorFactory.GetEditorOperations(TextView).MoveToNextCharacter(extend);
                    break;
            }

            return CommandResult.Executed;
        }

        private SnapshotPoint? MapUp(InteractiveWindow.IInteractiveWindow window, SnapshotPoint point) {
            return window.TextView.BufferGraph.MapUpToBuffer(
                point,
                Text.PointTrackingMode.Positive,
                Text.PositionAffinity.Successor,
                TextView.TextBuffer
            );
        }
    }
}
