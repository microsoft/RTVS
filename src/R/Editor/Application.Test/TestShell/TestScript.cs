// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.Languages.Editor.Shell;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.R.Editor.Application.Test.TestShell {
    [ExcludeFromCodeCoverage]
    public sealed partial class TestScript: IDisposable
    {
        /// <summary>
        /// Text content of the editor document
        /// </summary>
        public string EditorText { get { return EditorWindow.CoreEditor.Text; } }

        /// <summary>
        /// Editor text document object
        /// </summary>
        public ITextDocument TextDocument { get { return EditorWindow.CoreEditor.TextDocument; } }

        /// <summary>
        /// Editor text buffer
        /// </summary>
        public ITextBuffer TextBuffer { get { return TextDocument.TextBuffer; } }

        /// <summary>
        /// Full path to loaded file, if any
        /// </summary>
        public string FilePath { get; private set; }

        #region Constructors
        public TestScript(string contentType)
        {
            EditorWindow.Create(string.Empty, "filename", contentType);
        }

        /// <summary>
        /// Create script with editor window prepopulated with a given content
        /// </summary>
        public TestScript(string text, string contentType)
        {
            EditorWindow.Create(text, "filename", contentType);
        }

        /// <summary>
        /// Create script that opens a disk file in an editor window
        /// </summary>
        /// <param name="fileName">File name</param>
        public TestScript(DeployFilesFixture fixture, string fileName, bool unused)
        {
            OpenFile(fixture, fileName);
        }
        #endregion

        /// <summary>
        /// Open a disk file in an editor window
        /// </summary>
        /// <param name="fixture"></param>
        /// <param name="fileName">File name</param>
        /// <returns>Editor instance</returns>
        public void OpenFile(DeployFilesFixture fixture, string fileName) {
            string content = fixture.LoadDestinationFile(fileName);
            EditorWindow.Create(content, fileName);
        }

        /// <summary>
        /// Executes a single command from VS2K set
        /// </summary>
        /// <param name="id">command id</param>
        /// <param name="msIdle">Timeout to pause before and after execution</param>
        public void Execute(VSConstants.VSStd2KCmdID id, int msIdle = 0)
        {
            Execute(VSConstants.VSStd2K, (int)id, null, msIdle);
        }

        /// <summary>
        /// Executes a single command
        /// </summary>
        /// <param name="group">Command group</param>
        /// <param name="id">command id</param>
        /// <param name="msIdle">Timeout to pause before and after execution</param>
        public void Execute(Guid group, int id, object commandData = null, int msIdle = 0)
        {
            EditorWindow.ExecCommand(group, id, commandData, msIdle);
        }

        /// <summary>
        /// Invokes a particular action in the editor window
        /// </summary>
        public void Invoke(Action action)
        {
            UIThreadHelper.Instance.Invoke(action);
        }

        /// <summary>
        /// Executes editor idle loop
        /// </summary>
        /// <param name="ms">Milliseconds to run idle</param>
        public void DoIdle(int ms = 100)
        {
            EditorWindow.DoIdle(ms);
        }

        /// <summary>
        /// Terminates script and closes editor window
        /// </summary>
        private void Close()
        {
            EditorWindow.Close();
        }

        /// <summary>
        /// Selects range in the editor view
        /// </summary>
        public void Select(int start, int length)
        {
            EditorWindow.Select(start, length);
        }

        /// <summary>
        /// Selects range in the editor view
        /// </summary>
        public void Select(int startLine, int startColumn, int endLine, int endColumn)
        {
            EditorWindow.Select(startLine, startColumn, endLine, endColumn);
        }

        public IList<ClassificationSpan> GetClassificationSpans()
        {
            var classifierAggregator = EditorShell.Current.ExportProvider.GetExport<IClassifierAggregatorService>().Value;
            var textBuffer = EditorWindow.CoreEditor.View.TextBuffer;
            var classifier = classifierAggregator.GetClassifier(textBuffer);
            var snapshot = textBuffer.CurrentSnapshot;
            return classifier.GetClassificationSpans(new SnapshotSpan(snapshot, 0, snapshot.Length));
        }

        public string WriteClassifications(IList<ClassificationSpan> classifications)
        {
            var sb = new StringBuilder();

            foreach (var c in classifications)
            {
                sb.Append('[');
                sb.Append(c.Span.Start.Position.ToString());
                sb.Append(':');
                sb.Append(c.Span.Length);
                sb.Append(']');
                sb.Append(' ');
                sb.Append(c.ClassificationType.ToString());
                sb.Append('\r');
                sb.Append('\n');
            }

            return sb.ToString();
        }

        public ISignatureHelpSession GetSignatureSession() {
            ISignatureHelpBroker broker = EditorShell.Current.ExportProvider.GetExportedValue<ISignatureHelpBroker>();
            var sessions = broker.GetSessions(EditorWindow.CoreEditor.View);
            ISignatureHelpSession session = sessions.FirstOrDefault();

            int retries = 0;
            while (session == null && retries < 10) {
                this.DoIdle(1000);
                sessions = broker.GetSessions(EditorWindow.CoreEditor.View);
                session = sessions.FirstOrDefault();
                retries++;
            }

            return session;
        }

        public ICompletionSession GetCompletionSession() {
            ICompletionBroker broker = EditorShell.Current.ExportProvider.GetExportedValue<ICompletionBroker>();
            var sessions = broker.GetSessions(EditorWindow.CoreEditor.View);
            ICompletionSession session = sessions.FirstOrDefault();

            int retries = 0;
            while (session == null && retries < 10) {
                this.DoIdle(1000);
                sessions = broker.GetSessions(EditorWindow.CoreEditor.View);
                session = sessions.FirstOrDefault();
                retries++;
            }

            return session;
        }

        public IList<IMappingTagSpan<IErrorTag>> GetErrorTagSpans() {
            var aggregatorService = EditorShell.Current.ExportProvider.GetExport<IViewTagAggregatorFactoryService>().Value;
            var tagAggregator = aggregatorService.CreateTagAggregator<IErrorTag>(EditorWindow.CoreEditor.View);
            var textBuffer = EditorWindow.CoreEditor.View.TextBuffer;
            return tagAggregator.GetTags(new SnapshotSpan(textBuffer.CurrentSnapshot, new Span(0, textBuffer.CurrentSnapshot.Length))).ToList();
        }

        public IList<IMappingTagSpan<IOutliningRegionTag>> GetOutlineTagSpans() {
            var aggregatorService = EditorShell.Current.ExportProvider.GetExport<IViewTagAggregatorFactoryService>().Value;
            var tagAggregator = aggregatorService.CreateTagAggregator<IOutliningRegionTag>(EditorWindow.CoreEditor.View);
            var textBuffer = EditorWindow.CoreEditor.View.TextBuffer;
            return tagAggregator.GetTags(new SnapshotSpan(textBuffer.CurrentSnapshot, new Span(0, textBuffer.CurrentSnapshot.Length))).ToList();
        }

        public string WriteErrorTags(IList<IMappingTagSpan<IErrorTag>> tags) {
            var sb = new StringBuilder();

            foreach (var c in tags) {
                IMappingSpan span = c.Span;
                SnapshotPoint? ptStart = span.Start.GetPoint(span.AnchorBuffer, PositionAffinity.Successor);
                SnapshotPoint? ptEnd = span.End.GetPoint(span.AnchorBuffer, PositionAffinity.Successor);
                sb.Append('[');
                sb.Append(ptStart.Value.Position);
                sb.Append(" - ");
                sb.Append(ptEnd.Value.Position);
                sb.Append(']');
                sb.Append(' ');
                sb.Append(c.Tag.ToolTipContent);
                sb.Append('\r');
                sb.Append('\n');
            }

            return sb.ToString();
        }


        public void Dispose() {
            Close();
        }
    }
}
