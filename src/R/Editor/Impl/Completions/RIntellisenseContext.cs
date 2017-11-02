// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Disposables;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Tree;

namespace Microsoft.R.Editor.Completions {
    /// <summary>
    /// R completion context. Provides information about current document, 
    /// caret position and other necessary data for the completion engine.
    /// </summary>
    public sealed class RIntellisenseContext : IntellisenseContext, IRIntellisenseContext {
        private readonly object _lock = new object();
        private readonly IREditorTree _editorTree;
        private AstRoot _lockedAstRoot;
        private int _lockCount;

        public bool InternalFunctions { get; set; }
        public bool AutoShownCompletion { get; }
        public bool IsRHistoryRequest { get; }

        public RIntellisenseContext(IEditorIntellisenseSession session, IEditorBuffer editorBuffer, IREditorTree editorTree, int position, bool autoShown = true, bool isRHistoryRequest = false) : 
            base(session, editorBuffer, position) {
            _editorTree = editorTree;
            AutoShownCompletion = autoShown;
            IsRHistoryRequest = isRHistoryRequest;
        }

        public RIntellisenseContext(IEditorIntellisenseSession session, IEditorBuffer editorBuffer, AstRoot ast, int position, bool autoShown = true, bool isRHistoryRequest = false) :
            base(session, editorBuffer, position) {
            _lockedAstRoot = ast;
            AutoShownCompletion = autoShown;
            IsRHistoryRequest = isRHistoryRequest;
        }

        public AstRoot AstRoot => _lockedAstRoot ?? _editorTree.AstRoot;

        public IDisposable AstReadLock() {
            lock (_lock) {
                var guid = Guid.NewGuid();

                if (_lockedAstRoot == null) {
                    _lockedAstRoot = _editorTree.AcquireReadLock(guid);
                }
                _lockCount++;

                return Disposable.Create(() => {
                    _lockCount--;
                    if (_lockCount == 0) {
                        _editorTree?.ReleaseReadLock(guid);
                        _lockedAstRoot = null;
                    }
                });
            }
        }
    }
}
