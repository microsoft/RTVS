// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.R.Editor.Application.Test {
    public interface IEditorScript : IDisposable {
        /// <summary>
        /// Text content of the editor document
        /// </summary>
        string EditorText { get; }

        /// <summary>
        /// Editor view
        /// </summary>
        IWpfTextView View { get; }

        /// <summary>
        /// Editor text document object
        /// </summary>
        ITextDocument TextDocument { get; }

        /// <summary>
        /// Editor text buffer
        /// </summary>
        ITextBuffer TextBuffer { get; }

        /// <summary>
        /// Simulates typing in the editor
        /// </summary>
        IEditorScript Type(string textToType, int idleTime = 10);
        IEditorScript DoIdle(int ms = 100);

        /// <summary>
        /// Move caret down by a number character
        /// </summary>
        IEditorScript MoveDown(int count = 1);

        /// <summary>
        /// Move caret up by a number character
        /// </summary>
        IEditorScript MoveUp(int count = 1);

        /// <summary>
        /// Move caret left by a number character
        /// </summary>
        IEditorScript MoveLeft(int count = 1);

        /// <summary>
        /// Move caret right by a number character
        /// </summary>
        IEditorScript MoveRight(int count = 1);

        /// <summary>
        /// Adds 'go to line/column' command to the command script
        /// </summary>
        /// <param name="line">Line number</param>
        /// <param name="column">Column number</param>
        IEditorScript GoTo(int line, int column);

        IEditorScript Enter();
        IEditorScript Backspace();
        IEditorScript Delete();
        IEditorScript Invoke(Action action);

        /// <summary>
        /// Selects range in the editor view
        /// </summary>
        IEditorScript Select(int start, int length);

        /// <summary>
        /// Selects range in the editor view
        /// </summary>
        IEditorScript Select(int startLine, int startColumn, int endLine, int endColumn);

        /// <summary>
        /// Executes a single command from VS2K set
        /// </summary>
        /// <param name="id">command id</param>
        /// <param name="msIdle">Timeout to pause before and after execution</param>
        IEditorScript Execute(VSConstants.VSStd2KCmdID id, int msIdle = 0);

        /// <summary>
        /// Executes a single command
        /// </summary>
        /// <param name="group">Command group</param>
        /// <param name="id">command id</param>
        /// <param name="commandData"></param>
        /// <param name="msIdle">Timeout to pause before and after execution</param>
        IEditorScript Execute(Guid group, int id, object commandData = null, int msIdle = 0);

        IEnumerable<ClassificationSpan> GetClassificationSpans();
        ICompletionSession GetCompletionSession();
        IList<IMappingTagSpan<IErrorTag>> GetErrorTagSpans();
        ILightBulbSession GetLightBulbSession();
        IList<IMappingTagSpan<IOutliningRegionTag>> GetOutlineTagSpans();
        ISignatureHelpSession GetSignatureSession();

        string WriteErrorTags(IList<IMappingTagSpan<IErrorTag>> tags);
    }
}