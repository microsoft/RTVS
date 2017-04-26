// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.Languages.Editor.Application.Controller {
    [ExcludeFromCodeCoverage]
    internal class BaseController : ICommandTarget {
        private ITextView _view;
        private IEditorOperations _editorOperations;
        private ITextBufferUndoManager _undoManager;
        private BraceCompletionCommandTarget _braceCompletionTarget;

        public void Initialize(ITextView view, IEditorOperations editorOperations, ITextBufferUndoManager undoManager, IServiceContainer services) {
            Debug.Assert(view != null, "view must not be null");
            Debug.Assert(editorOperations != null, "editor operations must not be null");

            _view = view;
            _editorOperations = editorOperations;
            _undoManager = undoManager;
            _braceCompletionTarget = new BraceCompletionCommandTarget(view, services);
        }

        #region ICommandTarget Members

        public CommandResult Invoke(Guid group, int id, object args, ref object outargs) {
            CommandResult result = _braceCompletionTarget.Invoke(group, id, args, ref outargs);
            if (result.WasExecuted) {
                return result;
            }

            if (group == VSConstants.VSStd2K) {
                switch (id) {
                    case (int)VSConstants.VSStd2KCmdID.TYPECHAR:
                        string text;
                        if (args is char) {
                            text = args.ToString();
                        } else {
                            text = Char.ConvertFromUtf32((System.UInt16)args);
                        }

                        result = this.PerformEditAction(() => _editorOperations.InsertText(text));
                        break;
                    case (int)VSConstants.VSStd2KCmdID.BACKSPACE:
                        result = this.PerformEditAction(() => _editorOperations.Backspace());
                        break;
                    case (int)VSConstants.VSStd2KCmdID.DELETE:
                        result = this.PerformEditAction(() => _editorOperations.Delete());
                        break;
                    case (int)VSConstants.VSStd2KCmdID.CANCEL:
                        _editorOperations.ResetSelection();
                        break;
                    case (int)VSConstants.VSStd2KCmdID.DOWN_EXT:
                        _editorOperations.MoveLineDown(true);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.DOWN:
                        _editorOperations.MoveLineDown(false);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.RIGHT_EXT:
                        _editorOperations.MoveToNextCharacter(true);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.RIGHT:
                        _editorOperations.MoveToNextCharacter(false);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.LEFT_EXT:
                        _editorOperations.MoveToPreviousCharacter(true);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.LEFT:
                        _editorOperations.MoveToPreviousCharacter(false);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.UP_EXT:
                        _editorOperations.MoveLineUp(true);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.UP:
                        _editorOperations.MoveLineUp(false);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.HOME_EXT:
                        _editorOperations.MoveToHome(true);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.HOME:
                        _editorOperations.MoveToHome(false);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.PAGEUP_EXT:
                        _editorOperations.PageUp(true);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.PAGEUP:
                        _editorOperations.PageUp(false);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.PAGEDN_EXT:
                        _editorOperations.PageDown(true);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.PAGEDN:
                        _editorOperations.PageDown(false);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.END_EXT:
                        _editorOperations.MoveToEndOfLine(true);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.END:
                        _editorOperations.MoveToEndOfLine(false);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.BACKTAB:
                        result = this.PerformEditAction(() => _editorOperations.Unindent());
                        break;
                    case (int)VSConstants.VSStd2KCmdID.TAB:
                        result = this.PerformEditAction(() => _editorOperations.Indent());
                        break;
                    case (int)VSConstants.VSStd2KCmdID.RETURN:
                        result = this.PerformEditAction(() => _editorOperations.InsertNewLine());
                        break;
                    case (int)VSConstants.VSStd2KCmdID.CTLMOVERIGHT:
                        _editorOperations.MoveToNextWord(true);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.CTLMOVELEFT:
                        _editorOperations.MoveToPreviousWord(true);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.TOPLINE_EXT:
                        _editorOperations.MoveToStartOfDocument(true);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.BOTTOMLINE_EXT:
                        _editorOperations.MoveToEndOfDocument(true);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.INSERT:
                        bool isEnabled = _editorOperations.Options.IsOverwriteModeEnabled();
                        _editorOperations.Options.SetOptionValue(DefaultTextViewOptions.OverwriteModeId, !isEnabled);
                        break;

                    case (int)VSConstants.VSStd2KCmdID.DELETEWORDLEFT:
                        result = this.PerformEditAction(() => _editorOperations.DeleteWordToLeft());
                        break;
                    case (int)VSConstants.VSStd2KCmdID.DELETEWORDRIGHT:
                        result = this.PerformEditAction(() => _editorOperations.DeleteWordToRight());
                        break;
                    case (int)VSConstants.VSStd2KCmdID.SELECTALL:
                        _editorOperations.SelectAll();
                        break;
                    case (int)VSConstants.VSStd2KCmdID.SELECTCURRENTWORD:
                        _editorOperations.SelectCurrentWord();
                        break;
                    case (int)VSConstants.VSStd2KCmdID.WORDNEXT:
                        _editorOperations.MoveToNextWord(false);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.WORDPREV:
                        _editorOperations.MoveToPreviousWord(false);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.TOPLINE:
                        _editorOperations.MoveToStartOfDocument(false);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.BOTTOMLINE:
                        _editorOperations.MoveToEndOfDocument(false);
                        break;
                    case (int)VSConstants.VSStd2KCmdID.SCROLLUP:
                        _editorOperations.ScrollUpAndMoveCaretIfNecessary();
                        break;
                    case (int)VSConstants.VSStd2KCmdID.SCROLLDN:
                        _editorOperations.ScrollDownAndMoveCaretIfNecessary();
                        break;
                    case (int)VSConstants.VSStd2KCmdID.COPY:
                        _editorOperations.CopySelection();
                        break;
                    case (int)VSConstants.VSStd2KCmdID.CUT:
                        return this.PerformEditAction(() => _editorOperations.CutSelection());
                    case (int)VSConstants.VSStd2KCmdID.PASTE:
                        string pastedText = args as string;

                        if (pastedText != null) {
                            return this.PerformEditAction(() => _editorOperations.InsertText(pastedText));
                        } else {
                            return this.PerformEditAction(() => _editorOperations.Paste());
                        }

                    case (int)VSConstants.VSStd2KCmdID.UNDO:

                        if (UndoManager != null &&
                            UndoManager.TextBufferUndoHistory.CanUndo) {
                            UndoManager.TextBufferUndoHistory.Undo(1);
                            break;
                        }

                        return CommandResult.Disabled;

                    case (int)VSConstants.VSStd2KCmdID.REDO:

                        if (UndoManager != null &&
                            UndoManager.TextBufferUndoHistory.CanRedo) {
                            UndoManager.TextBufferUndoHistory.Redo(1);
                            break;
                        }

                        return CommandResult.Disabled;
                    default:
                        return CommandResult.NotSupported;
                }

                _braceCompletionTarget.PostProcessInvoke(CommandResult.Executed, group, id, args, ref outargs);
                return result;
            }

            return CommandResult.NotSupported;
        }

        public void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg) {
        }

        public CommandStatus Status(Guid group, int id) {
            if (group == VSConstants.VSStd2K) {
                switch (id) {
                    // can performEditAction
                    case (int)VSConstants.VSStd2KCmdID.TYPECHAR:
                    case (int)VSConstants.VSStd2KCmdID.BACKSPACE:
                    case (int)VSConstants.VSStd2KCmdID.BACKTAB:
                    case (int)VSConstants.VSStd2KCmdID.TAB:
                    case (int)VSConstants.VSStd2KCmdID.RETURN:
                    case (int)VSConstants.VSStd2KCmdID.DELETEWORDLEFT:
                    case (int)VSConstants.VSStd2KCmdID.DELETEWORDRIGHT:
                        return CanPerformEditAction();
                    case (int)VSConstants.VSStd2KCmdID.DELETE:

                        if (_editorOperations.CanDelete) {
                            return CommandStatus.SupportedAndEnabled;
                        } else {
                            return CommandStatus.Supported;
                        }

                    case (int)VSConstants.VSStd2KCmdID.CUT:

                        if (_editorOperations.CanCut) {
                            return CommandStatus.SupportedAndEnabled;
                        } else {
                            return CommandStatus.Supported;
                        }

                    case (int)VSConstants.VSStd2KCmdID.PASTE:

                        if (_editorOperations.CanPaste) {
                            return CommandStatus.SupportedAndEnabled;
                        } else {
                            return CommandStatus.Supported;
                        }

                    case (int)VSConstants.VSStd2KCmdID.CANCEL:
                    case (int)VSConstants.VSStd2KCmdID.DOWN_EXT:
                    case (int)VSConstants.VSStd2KCmdID.DOWN:
                    case (int)VSConstants.VSStd2KCmdID.RIGHT_EXT:
                    case (int)VSConstants.VSStd2KCmdID.RIGHT:
                    case (int)VSConstants.VSStd2KCmdID.LEFT_EXT:
                    case (int)VSConstants.VSStd2KCmdID.LEFT:
                    case (int)VSConstants.VSStd2KCmdID.UP_EXT:
                    case (int)VSConstants.VSStd2KCmdID.UP:
                    case (int)VSConstants.VSStd2KCmdID.HOME_EXT:
                    case (int)VSConstants.VSStd2KCmdID.HOME:
                    case (int)VSConstants.VSStd2KCmdID.PAGEUP_EXT:
                    case (int)VSConstants.VSStd2KCmdID.PAGEUP:
                    case (int)VSConstants.VSStd2KCmdID.PAGEDN_EXT:
                    case (int)VSConstants.VSStd2KCmdID.PAGEDN:
                    case (int)VSConstants.VSStd2KCmdID.END_EXT:
                    case (int)VSConstants.VSStd2KCmdID.END:
                    case (int)VSConstants.VSStd2KCmdID.CTLMOVERIGHT:
                    case (int)VSConstants.VSStd2KCmdID.CTLMOVELEFT:
                    case (int)VSConstants.VSStd2KCmdID.TOPLINE_EXT:
                    case (int)VSConstants.VSStd2KCmdID.BOTTOMLINE_EXT:
                    case (int)VSConstants.VSStd2KCmdID.INSERT:
                    case (int)VSConstants.VSStd2KCmdID.SELECTALL:
                    case (int)VSConstants.VSStd2KCmdID.SELECTCURRENTWORD:
                    case (int)VSConstants.VSStd2KCmdID.WORDNEXT:
                    case (int)VSConstants.VSStd2KCmdID.WORDPREV:
                    case (int)VSConstants.VSStd2KCmdID.TOPLINE:
                    case (int)VSConstants.VSStd2KCmdID.BOTTOMLINE:
                    case (int)VSConstants.VSStd2KCmdID.SCROLLUP:
                    case (int)VSConstants.VSStd2KCmdID.SCROLLDN:
                    case (int)VSConstants.VSStd2KCmdID.COPY:
                        return CommandStatus.SupportedAndEnabled;
                    case (int)VSConstants.VSStd2KCmdID.UNDO:

                        if (UndoManager != null &&
                            UndoManager.TextBufferUndoHistory.CanUndo) {
                            return CommandStatus.SupportedAndEnabled;
                        } else {
                            return CommandStatus.Supported;
                        }

                    case (int)VSConstants.VSStd2KCmdID.REDO:

                        if (UndoManager != null &&
                            UndoManager.TextBufferUndoHistory.CanRedo) {
                            return CommandStatus.SupportedAndEnabled;
                        } else {
                            return CommandStatus.Supported;
                        }
                }
            }

            return CommandStatus.NotSupported;
        }

        #endregion

        /// <summary>
        /// Performs the passed editAction if the view does not prohibit user input.
        /// </summary>
        /// <returns>True if the editAction was performed.</returns>
        private CommandResult PerformEditAction(Action editAction) {
            if (!_view.Options.GetOptionValue<bool>(DefaultTextViewOptions.ViewProhibitUserInputId)) {
                editAction.Invoke();
                return CommandResult.Executed;
            }

            return CommandResult.Disabled;
        }

        private CommandStatus CanPerformEditAction() {
            if (!_view.Options.GetOptionValue<bool>(DefaultTextViewOptions.ViewProhibitUserInputId)) {
                return CommandStatus.SupportedAndEnabled;
            }

            return CommandStatus.Supported;
        }

        private ITextBufferUndoManager UndoManager { get { return _undoManager; } }
    }
}
