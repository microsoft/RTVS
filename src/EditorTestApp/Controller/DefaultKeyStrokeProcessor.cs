// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.Languages.Editor.Application.Controller {
    [ExcludeFromCodeCoverage]
    internal sealed class DefaultKeyProcessor : KeyProcessor {
        private IWpfTextView _textView;
        private IEditorOperations _editorOperations;
        private ITextUndoHistoryRegistry _undoHistoryRegistry;
        private ICommandTarget _controller;
        private KeyToVS2KCommandMapping _commandMapping;

        private ICommandTarget Controller {
            get {
                _controller = _controller ?? _textView.GetService<ICommandTarget>();
                return _controller;
            }
        }

        private KeyToVS2KCommandMapping CommandMapping {
            get {
                _commandMapping = _commandMapping ?? KeyToVS2KCommandMapping.GetInstance();
                return _commandMapping;
            }
        }

        internal DefaultKeyProcessor(IWpfTextView textView, IEditorOperations editorOperations, ITextUndoHistoryRegistry undoHistoryRegistry) {
            Check.ArgumentNull(nameof(textView), textView);
            Check.ArgumentNull(nameof(editorOperations), editorOperations);
            Check.ArgumentNull(nameof(undoHistoryRegistry), undoHistoryRegistry);

            _textView = textView;
            _editorOperations = editorOperations;
            _undoHistoryRegistry = undoHistoryRegistry;
        }

        public override void KeyDown(KeyEventArgs args) {
            args.Handled = true;

            VSConstants.VSStd2KCmdID cmdId;
            if (CommandMapping.TryGetValue(args.KeyboardDevice.Modifiers, args.Key, out cmdId)) {
                args.Handled = TryExecute2KCommand(cmdId, null).WasExecuted;
            } else {
                switch (args.KeyboardDevice.Modifiers) {
                    case ModifierKeys.None:
                        HandleKey(args);
                        break;
                    case ModifierKeys.Control:
                        HandleControlKey(args);
                        break;
                    case ModifierKeys.Alt:
                        HandleAltKey(args);
                        break;
                    case ModifierKeys.Shift | ModifierKeys.Alt:
                        HandleAltShiftKey(args);
                        break;
                    case ModifierKeys.Control | ModifierKeys.Shift:
                        HandleControlShiftKey(args);
                        break;
                    case ModifierKeys.Shift:
                        HandleShiftKey(args);
                        break;

                    default:
                        args.Handled = false;
                        break;
                }
            }
        }

        private void HandleShiftKey(KeyEventArgs args) {
            // All original shift commands are handled through the KeyToCommandMappingClass
            args.Handled = false;
        }

        private void HandleControlShiftKey(KeyEventArgs args) {
            switch (args.Key) {
                case Key.U:
                    args.Handled = this.PerformEditAction(() => _editorOperations.MakeUppercase());
                    break;
                default:
                    args.Handled = false;
                    break;
            }
        }

        private void HandleAltShiftKey(KeyEventArgs args) {
            if (args.Key == Key.T) {
                args.Handled = this.PerformEditAction(() => _editorOperations.TransposeLine());
                return;
            }

            // If this is starting a new selection, put the selection in
            // box selection mode.
            if ((args.Key == Key.Down ||
                 args.Key == Key.Up ||
                 args.Key == Key.Left ||
                 args.Key == Key.Right) &&
                _textView.Selection.IsEmpty) {
                _textView.Selection.Mode = TextSelectionMode.Box;
            }

            // TODO: re-route the shift keys correctly
            // currently these will get lost because they don't get mapped correctly
            HandleShiftKey(args);
        }

        private void HandleAltKey(KeyEventArgs args) {
            switch (args.Key) {
                case Key.Left:
                    _editorOperations.SelectEnclosing();
                    break;
                case Key.Right:
                    _editorOperations.SelectFirstChild();
                    break;
                case Key.Down:
                    _editorOperations.SelectNextSibling(false);
                    break;
                case Key.Up:
                    _editorOperations.SelectPreviousSibling(false);
                    break;
                default:
                    args.Handled = false;
                    break;
            }
        }

        private void HandleControlKey(KeyEventArgs args) {
            switch (args.Key) {
                case Key.T:
                    args.Handled = this.PerformEditAction(() => _editorOperations.TransposeCharacter());
                    break;
                case Key.U:
                    args.Handled = this.PerformEditAction(() => _editorOperations.MakeLowercase());
                    break;
                default:
                    args.Handled = false;
                    break;
            }
        }

        private void HandleKey(KeyEventArgs args) {
            args.Handled = false;
        }

        private bool CanExecute(Guid group, int id) {
            Debug.Assert(id >= 0, "Id must be positive");
            var status = CommandStatus.NotSupported;

            if (Controller != null && id > 0) {
                status = Controller.Status(group, id);
            }
            return ((status & CommandStatus.SupportedAndEnabled) == CommandStatus.SupportedAndEnabled);
        }

        private CommandResult TryExecute2KCommand(VSConstants.VSStd2KCmdID id, object args) => TryExecute(VSConstants.VSStd2K, (int)id, args);

        private CommandResult TryExecute(Guid group, int id, object args) {
            if (Controller != null && (Controller.Status(group, id) & CommandStatus.SupportedAndEnabled) == CommandStatus.SupportedAndEnabled) {
                var outargs = new object();
                return Controller.Invoke(group, id, args, ref outargs);
            }
            return CommandResult.NotSupported;
        }

        public override void TextInput(TextCompositionEventArgs args) {
            // The view will generate an text input event of length zero to flush the current provisional composition span.
            // No one else should be doing that, so ignore zero length inputs unless there is provisional text to flush.
            if ((args.Text.Length > 0) || (_editorOperations.ProvisionalCompositionSpan != null)) {
                if (args.Text.Length == 1) {
                    var cr = TryExecute2KCommand(VSConstants.VSStd2KCmdID.TYPECHAR, args.Text[0]);
                    args.Handled = cr.WasExecuted;
                } else {
                    args.Handled = this.PerformEditAction(() => _editorOperations.InsertText(args.Text));
                }

                if (args.Handled) {
                    _textView.Caret.EnsureVisible();
                }
            }
        }

        public override void TextInputStart(TextCompositionEventArgs args) {
            if (args.TextComposition is ImeTextComposition) {
                //This TextInputStart message is part of an IME event and needs to be treated like provisional text input
                //(if the cast failed, then an IME is not the source of the text input and we can rely on getting an identical
                //TextInput event as soon as we exit).
                HandleProvisionalImeInput(args);
            }
        }

        public override void TextInputUpdate(TextCompositionEventArgs args) {
            if (args.TextComposition is ImeTextComposition) {
                HandleProvisionalImeInput(args);
            } else {
                args.Handled = false;
            }
        }

        private void HandleProvisionalImeInput(TextCompositionEventArgs args) {
            if (args.Text.Length > 0) {
                args.Handled = this.PerformEditAction(() => _editorOperations.InsertProvisionalText(args.Text));

                if (args.Handled) {
                    _textView.Caret.EnsureVisible();
                }
            }
        }

        /// <summary>
        /// Performs the passed editAction if the view does not prohibit user input.
        /// </summary>
        /// <returns>True if the editAction was performed.</returns>
        private bool PerformEditAction(Action editAction) {
            if (!_textView.Options.GetOptionValue<bool>(DefaultTextViewOptions.ViewProhibitUserInputId)) {
                editAction.Invoke();
                return true;
            }
            return false;
        }

        private ITextUndoHistory UndoHistor => _undoHistoryRegistry.GetHistory(_textView.TextBuffer);
    }
}
