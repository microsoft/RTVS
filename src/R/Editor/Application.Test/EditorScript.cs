// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Languages.Editor.Application.Core;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.Languages.Editor.Shell;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.R.Editor.Application.Test {
    internal class EditorScript : IEditorScript {
        private const int _defaultTypingTimeout = 25;
        private readonly IExportProvider _exportProvider;
        private readonly CoreEditor _coreEditor;
        private readonly IDisposable _containerDisposable;
        private readonly IEditorShell _editorShell;

        public EditorScript(IExportProvider exportProvider, CoreEditor coreEditor, IDisposable containerDisposable) {
            _exportProvider = exportProvider;
            _coreEditor = coreEditor;
            _editorShell = _exportProvider.GetExportedValue<IEditorShell>();
            _containerDisposable = containerDisposable;
        }

        public void Dispose() {
            _containerDisposable.Dispose();
            _coreEditor.Close();
        }

        /// <summary>
        /// Editor text document object
        /// </summary>
        public ITextDocument TextDocument => _coreEditor.TextDocument;

        /// <summary>
        /// Editor text buffer
        /// </summary>
        public ITextBuffer TextBuffer => TextDocument.TextBuffer;

        /// <summary>
        /// Text content of the editor document
        /// </summary>
        public string EditorText => _coreEditor.Text;

        public IEditorScript Type(string textToType, int idleTime = _defaultTypingTimeout) {
            for (var i = 0; i < textToType.Length; i++) {
                char ch = textToType[i];

                int length = TranslateSpecialChar(textToType, i, idleTime);
                if (length > 0) {
                    i += length;
                } else {
                    Execute(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.TYPECHAR, ch, idleTime);
                    if (idleTime > 0) {
                        DoIdle(idleTime);
                    }
                }
            }

            return this;
        }

        private int TranslateSpecialChar(string textToType, int index, int idleTime) {
            var length = 0;

            if (index < textToType.Length - 1 && textToType[index] == '{' && char.IsUpper(textToType[index + 1])) {
                index++;

                var closeBrace = textToType.IndexOf('}', index);
                if (closeBrace < 0) {
                    return length;
                }

                length = closeBrace - index + 1;
                var s = textToType.Substring(index, closeBrace - index);

                switch (s) {
                    case "ENTER":
                        Execute(VSConstants.VSStd2KCmdID.RETURN, idleTime);
                        break;
                    case "TAB":
                        Execute(VSConstants.VSStd2KCmdID.TAB, idleTime);
                        break;
                    case "BACKSPACE":
                        Execute(VSConstants.VSStd2KCmdID.BACKSPACE, idleTime);
                        break;
                    case "DELETE":
                        Execute(VSConstants.VSStd2KCmdID.DELETE, idleTime);
                        break;
                    case "UP":
                        Execute(VSConstants.VSStd2KCmdID.UP, idleTime);
                        break;
                    case "DOWN":
                        Execute(VSConstants.VSStd2KCmdID.DOWN, idleTime);
                        break;
                    case "LEFT":
                        Execute(VSConstants.VSStd2KCmdID.LEFT, idleTime);
                        break;
                    case "RIGHT":
                        Execute(VSConstants.VSStd2KCmdID.RIGHT, idleTime);
                        break;
                    case "HOME":
                        Execute(VSConstants.VSStd2KCmdID.HOME, idleTime);
                        break;
                    case "END":
                        Execute(VSConstants.VSStd2KCmdID.END, idleTime);
                        break;
                    case "ESC":
                        Execute(VSConstants.VSStd2KCmdID.CANCEL, idleTime);
                        break;
                }
            }

            return length;
        }

        public IEditorScript DoIdle(int ms = 100) {
            UIThreadHelper.Instance.Invoke(() => {
                for (var i = 0; i < ms; i += 20) {
                    _editorShell.DoIdle();
                    Thread.Sleep(20);
                }
            });
            return this;
        }

        public IEditorScript MoveDown(int count = 1) => Execute(VSConstants.VSStd2KCmdID.DOWN, count, 0);

        public IEditorScript MoveUp(int count = 1) => Execute(VSConstants.VSStd2KCmdID.UP, count, 0);

        public IEditorScript MoveLeft(int count = 1) => Execute(VSConstants.VSStd2KCmdID.LEFT, count, 0);

        public IEditorScript MoveRight(int count = 1) => Execute(VSConstants.VSStd2KCmdID.RIGHT, count, 0);

        public IEditorScript GoTo(int line, int column) => Invoke(() => _coreEditor.GoTo(line, column, 0));

        public IEditorScript Enter() => Execute(VSConstants.VSStd2KCmdID.RETURN, _defaultTypingTimeout);

        public IEditorScript Backspace() => Execute(VSConstants.VSStd2KCmdID.BACKSPACE, _defaultTypingTimeout);

        public IEditorScript Delete() => Execute(VSConstants.VSStd2KCmdID.DELETE, _defaultTypingTimeout);

        public IEditorScript Invoke(Action action) {
            UIThreadHelper.Instance.Invoke(action);
            return this;
        }

        public IEditorScript Select(int start, int length) => Invoke(() => {
            var selection = _coreEditor.View.Selection;
            var snapshot = _coreEditor.View.TextBuffer.CurrentSnapshot;

            selection.Select(new SnapshotSpan(snapshot, start, length), isReversed: false);
        });

        public IEditorScript Select(int startLine, int startColumn, int endLine, int endColumn) => Invoke(() => {
            var selection = _coreEditor.View.Selection;
            var snapshot = _coreEditor.View.TextBuffer.CurrentSnapshot;

            var line1 = snapshot.GetLineFromLineNumber(startLine);
            var start = line1.Start + startColumn;

            var line2 = snapshot.GetLineFromLineNumber(endLine);
            var end = line2.Start + endColumn;

            selection.Select(new SnapshotSpan(snapshot, start, end - start), isReversed: false);
        });

        public IEditorScript Execute(VSConstants.VSStd2KCmdID id, int msIdle = 0) => Execute(VSConstants.VSStd2K, (int)id, null, msIdle);

        public IEditorScript Execute(Guid @group, int id, object commandData = null, int msIdle = 0) => Invoke(() => {
            var unused = new object();
            _coreEditor.CommandTarget.Invoke(@group, id, commandData, ref unused);
            Thread.Sleep(msIdle);
        });

        private IEditorScript Execute(VSConstants.VSStd2KCmdID cmdId, int count, int msIdle) {
            for (var i = 0; i < count; i++) {
                Execute(cmdId, msIdle);
            }

            return this;
        }
        public ICompletionSession GetCompletionSession() {
            var broker = _exportProvider.GetExportedValue<ICompletionBroker>();
            var sessions = broker.GetSessions(_coreEditor.View);
            var session = sessions.FirstOrDefault();

            int retries = 0;
            while (session == null && retries < 10) {
                DoIdle(1000);
                sessions = broker.GetSessions(_coreEditor.View);
                session = sessions.FirstOrDefault();
                retries++;
            }

            return session;
        }

        public IList<IMappingTagSpan<IErrorTag>> GetErrorTagSpans() {
            throw new NotImplementedException();
        }

        public ILightBulbSession GetLightBulbSession() {
            throw new NotImplementedException();
        }

        public IList<IMappingTagSpan<IOutliningRegionTag>> GetOutlineTagSpans() {
            throw new NotImplementedException();
        }

        public ISignatureHelpSession GetSignatureSession() {
            throw new NotImplementedException();
        }

        public string WriteErrorTags(IList<IMappingTagSpan<IErrorTag>> tags) {
            throw new NotImplementedException();
        }
    }
}