using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.R.Editor.Application.Test.TestShell
{
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
        public TestScript(TestContext context, string fileName, bool unused)
        {
            OpenFile(context, fileName);
        }
        #endregion

        /// <summary>
        /// Open a disk file in an editor window
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="contentType">File content type</param>
        /// <returns>Editor instance</returns>
        public void OpenFile(TestContext context, string fileName)
        {
            string content = TestFiles.LoadFile(context, fileName);
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
            EditorWindow.Invoke(action);
        }

        /// <summary>
        /// Invokes a callback function in the editor window and return result
        /// </summary>
        public object Invoke(EditorWindow.FunctionInvoke func, object param)
        {
            return EditorWindow.Invoke(func, param);
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

        public void Dispose() {
            Close();
        }
    }
}
