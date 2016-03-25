// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Navigation {
    public sealed class GoToDefinitionCommand : ViewCommand {
        private ITextBuffer _textBuffer;

        public GoToDefinitionCommand(ITextView textView, ITextBuffer textBuffer) :
           base(textView, new CommandId(typeof(VSConstants.VSStd97CmdID).GUID,
                (int)VSConstants.VSStd97CmdID.GotoDefn), needCheckout: false) {
            _textBuffer = textBuffer;
        }

        public override CommandStatus Status(Guid group, int id) {
            return CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            Span span;
            string itemName = TextView.GetIdentifierUnderCaret(out span);
            if (!string.IsNullOrEmpty(itemName)) {
                var document = REditorDocument.FromTextBuffer(_textBuffer);
                var ast = document.EditorTree.AstRoot;
                var position = REditorDocument.MapCaretPositionFromView(TextView);
                if (position.HasValue) {
                    int positionToNavigateTo = -1;
                    var scope = ast.GetScope();
                    var func = scope.FindFunctionByName(itemName, position.Value);
                    if (func != null) {
                        positionToNavigateTo = func.Value.Start;
                    } else {
                        var v = scope.FindVariableByName(itemName, position.Value);
                        if (v != null) {
                            positionToNavigateTo = v.Start;
                        }
                    }
                    if (positionToNavigateTo >= 0) {
                        var viewPoint = TextView.MapUpToBuffer(positionToNavigateTo, TextView.TextBuffer);
                        if (viewPoint.HasValue) {
                            TextView.Caret.MoveTo(new SnapshotPoint(TextView.TextBuffer.CurrentSnapshot, viewPoint.Value));
                            TextView.Caret.EnsureVisible();
                        }
                    }
                }
            }
            return CommandResult.NotSupported;
        }
    }
}
