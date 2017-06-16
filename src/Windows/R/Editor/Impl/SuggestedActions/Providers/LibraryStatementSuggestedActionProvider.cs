// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.SuggestedActions;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.SuggestedActions.Actions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.SuggestedActions.Providers {
    [Export(typeof(ISuggestedActionProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Library Statement Suggested Action Provider")]
    internal sealed class LibraryStatementSuggestedActionProvider : ISuggestedActionProvider {
        private readonly IRInteractiveWorkflowProvider _workflowProvider;

        [ImportingConstructor]
        public LibraryStatementSuggestedActionProvider(IRInteractiveWorkflowProvider workflowProvider) {
            _workflowProvider = workflowProvider;
        }

        private static readonly Guid _treeUsedId = new Guid("{F0C102DF-9B3E-4C69-9CFE-23C244DBC7C4}");
        public IEnumerable<ISuggestedAction> GetSuggestedActions(ITextView textView, ITextBuffer textBuffer, int bufferPosition) {
            var workflow = _workflowProvider.GetOrCreate();
            return new ISuggestedAction[] {
                new InstallPackageSuggestedAction(textView, textBuffer, workflow, bufferPosition),
                new LoadLibrarySuggestedAction(textView, textBuffer, workflow, bufferPosition)
            };
        }

        public bool HasSuggestedActions(ITextView textView, ITextBuffer textBuffer, int bufferPosition) {
            string libraryName = null;
            var doc = textBuffer.GetEditorDocument<IREditorDocument>();
            if(doc != null && doc.EditorTree.IsReady) {
                var ast = doc.EditorTree.AcquireReadLock(_treeUsedId);
                try {
                    libraryName = ast.IsInLibraryStatement(bufferPosition);
                }
                finally {
                    doc.EditorTree.ReleaseReadLock(_treeUsedId);
                }
            }
            return !string.IsNullOrEmpty(libraryName);
        }
    }
}
