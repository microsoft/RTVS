using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Windows;
using Microsoft.Common.Core.Test.STA;
using Microsoft.Languages.Editor.Application.Core;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.Text;
using Screen = System.Windows.Forms.Screen;

namespace Microsoft.R.Editor.Application.Test.TestShell {
    /// <summary>
    /// Visual Studio Editor window
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class EditorWindow {
        [ExcludeFromCodeCoverage]
        class EditorTestRequest {
            public string FileName { get; }
            public string Text { get; }
            public string ContentType { get; }

            public EditorTestRequest(string text, string fileName, string contentType) {
                FileName = fileName;
                Text = text;
                ContentType = contentType;
            }
        }

        /// <summary>
        /// Underlying core editor object
        /// </summary>
        public static CoreEditor CoreEditor { get; private set; }

        /// <summary>
        /// WPF window that contains core editor control
        /// </summary>
        public static Window Window { get; private set; }

        /// <summary>
        /// Editor text buffer
        /// </summary>
        public static ITextBuffer TextBuffer { get { return CoreEditor.TextDocument.TextBuffer; } }

        /// <summary>
        /// Creates an editor window. Editor window is a singleton and it is not possible
        /// to have more than one up at a time. Multiple threads can place requests for
        /// the editor window creation but requests will be seriazed and only one will be
        /// granted at a time. thread must call Close() to allow other threads to get
        /// access to the editor window. Note that all editor windows are created on a 
        /// separate STA thread which remains the same for all editors created.
        /// </summary>
        /// <param name="text">Initial editor text</param>
        /// <param name="fileName">File name</param>
        /// <param name="contentType">Content type of the text buffer</param>
        /// <returns></returns>
        public static void Create(string text, string fileName, string contentType = null) {
            StaThread.RunStaTest((o, evt) => {
                CreateEditorInstance(o as EditorTestRequest, evt);
            }, new EditorTestRequest(text, fileName, contentType));
        }

        private static void CreateEditorInstance(EditorTestRequest a, ManualResetEventSlim evt) {
            CoreEditor = new CoreEditor(a.Text, a.FileName, a.ContentType);

            Window = new Window();

            if (Screen.AllScreens.Length == 1) {
                Window.Left = 0;
                Window.Top = 50;
            } else {
                Screen secondary = Screen.AllScreens.FirstOrDefault(x => !x.Primary);
                Window.Left = secondary.WorkingArea.Left;
                Window.Top = secondary.WorkingArea.Top + 50;
            }

            Window.Width = 800;
            Window.Height = 600;

            Window.Title = "R Editor - " + (a.FileName != null ? a.FileName : "Untitled");
            Window.Content = CoreEditor.Control;

            evt.Set();

            Window.Topmost = true;
            Window.ShowDialog();
        }

        public static void ExecCommand(Guid group, int id, object commandData = null, int msIdle = 0) {
            var action = new Action(() => {
                var unused = new object();
                CoreEditor.CommandTarget.Invoke(group, id, commandData, ref unused);
                Thread.Sleep(msIdle);
            });

            StaThread.Invoke(action);
        }

        public static void DoIdle(int ms) {
            var action = new Action(() => {
                int time = 0;

                while (time < ms) {
                    EditorShell.Current.DoIdle();
                    Thread.Sleep(20);
                    time += 20;
                }
            });

            StaThread.Invoke(action);
        }

        /// <summary>
        /// Closes editor window
        /// </summary>
        public static void Close() {
            var action = new Action(() => {
                Window.Close();
                CoreEditor.Close();
                StaThread.ThreadAvailable.Set();
            });

            StaThread.Invoke(action);
        }

        /// <summary>
        /// Selects range in the editor view
        /// </summary>
        public static void Select(int start, int length) {
            var action = new Action(() => {
                var selection = CoreEditor.View.Selection;
                var snapshot = CoreEditor.View.TextBuffer.CurrentSnapshot;

                selection.Select(new SnapshotSpan(snapshot, start, length), isReversed: false);
            });

            StaThread.Invoke(action);
        }

        /// <summary>
        /// Selects range in the editor view
        /// </summary>
        public static void Select(int startLine, int startColumn, int endLine, int endColumn) {
            var action = new Action(() => {
                var selection = CoreEditor.View.Selection;
                var snapshot = CoreEditor.View.TextBuffer.CurrentSnapshot;

                var line1 = snapshot.GetLineFromLineNumber(startLine);
                var start = line1.Start + startColumn;

                var line2 = snapshot.GetLineFromLineNumber(endLine);
                var end = line2.Start + endColumn;

                selection.Select(new SnapshotSpan(snapshot, start, end - start), isReversed: false);
            });

            StaThread.Invoke(action);
        }
    }
}
