using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Languages.Editor.Application.Core;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Application.Test.TestShell
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public sealed class EditorTest
    {
        [AssemblyCleanup]
        public static void Cleanup()
        {
            EditorWindow.Terminate();
        }
    }

    /// <summary>
    /// Visual Studio Editor window
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class EditorWindow
    {
        [ExcludeFromCodeCoverage]
        class Request
        {
            public string FileName;
            public string Text;
            public string ContentType;

            public ManualResetEventSlim Event = new ManualResetEventSlim(false);

            public Request(string text, string fileName, string contentType)
            {
                Text = text;
                FileName = fileName;
                ContentType = contentType;
            }
        }

        public delegate object FunctionInvoke(object param);

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
        /// Editor UI thread
        /// </summary>
        private static Thread EditorThread;
        private static readonly object _creatorLock = new object();

        private static ManualResetEventSlim ThreadAvailable = new ManualResetEventSlim(false);
        private static ConcurrentQueue<Request> _requestQueue = new ConcurrentQueue<Request>();

        private static ManualResetEventSlim ThreadExit = new ManualResetEventSlim(false);

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
        public static void Create(string text, string fileName, string contentType = null)
        {
            var request = new Request(text, fileName, contentType);
            _requestQueue.Enqueue(request);

            if (!ThreadExit.IsSet)
            {
                lock (_creatorLock)
                {
                    if (EditorThread == null)
                    {
                        EditorThread = new Thread(EditorThreadEntry);
                        EditorThread.SetApartmentState(ApartmentState.STA);
                        EditorThread.IsBackground = false;
                        EditorThread.DisableComObjectEagerCleanup();

                        EditorThread.Start();
                        ThreadAvailable.Set();
                    }
                }
            }

            request.Event.Wait();
        }

        public static void Terminate()
        {
            if (!ThreadExit.IsSet)
            {
                ThreadExit.Set();
                EditorThread.Join();
            }
        }

        private static void EditorThreadEntry()
        {
            var timeStart = DateTime.Now;

            while (!ThreadExit.IsSet)
            {
                ThreadAvailable.Wait();

                if (_requestQueue.Count > 0)
                {
                    Request request;
                    _requestQueue.TryDequeue(out request);

                    ThreadAvailable.Reset();

                    CreateEditorInstance(request);
                }
                else
                {
                    ThreadExit.Wait(200);
                }
            }

            try
            {
                var dispatcher = CoreEditor.Control.Dispatcher;

                if (!dispatcher.HasShutdownStarted)
                    dispatcher.InvokeShutdown();

                while (!dispatcher.HasShutdownFinished)
                {
                    Thread.Sleep(10);
                }
            }
            catch (Exception) { }
        }

        private static void CreateEditorInstance(Request request)
        {
            CoreEditor = new CoreEditor(request.Text, request.FileName, request.ContentType);

            Window = new Window();

            Window.Width = 800;
            Window.Height = 600;

            Window.Title = "R Editor - " + (request.FileName != null ? request.FileName : "Untitled");
            Window.Content = CoreEditor.Control;

            request.Event.Set();

            Window.Topmost = true;
            Window.ShowDialog();
        }

        /// <summary>
        /// Invokes an action in the editor UI thread
        /// </summary>
        /// <param name="action"></param>
        public static void Invoke(Action action)
        {
            while (CoreEditor == null || CoreEditor.Control.Dispatcher == null)
            {
                Thread.Sleep(50);
            }

            CoreEditor.Control.Dispatcher.Invoke(action, DispatcherPriority.Input);
        }

        /// <summary>
        /// Invokes a function in the editor UI thread and returns function result
        /// </summary>
        public static object Invoke(FunctionInvoke function, object param)
        {
            return CoreEditor.Control.Dispatcher.Invoke(function, DispatcherPriority.Input, new object[] { param });
        }

        public static void ExecCommand(Guid group, int id, object commandData = null, int msIdle = 0)
        {
            var action = new Action(() =>
            {
                var unused = new object();
                CoreEditor.CommandTarget.Invoke(group, id, commandData, ref unused);
                Thread.Sleep(msIdle);
            });

            Invoke(action);
        }

        public static void DoIdle(int ms)
        {
            var action = new Action(() =>
             {
                 int time = 0;

                 while (time < ms)
                 {
                     EditorShell.Current.DoIdle();
                     Thread.Sleep(20);
                     time += 20;
                 }
             });

            Invoke(action);
        }

        /// <summary>
        /// Closes editor window
        /// </summary>
        public static void Close()
        {
            var action = new Action(() =>
                {
                    Window.Close();
                    CoreEditor.Close();
                    ThreadAvailable.Set();
                });

            Invoke(action);
        }

        /// <summary>
        /// Selects range in the editor view
        /// </summary>
        public static void Select(int start, int length)
        {
            var action = new Action(() =>
            {
                var selection = CoreEditor.View.Selection;
                var snapshot = CoreEditor.View.TextBuffer.CurrentSnapshot;

                selection.Select(new SnapshotSpan(snapshot, start, length), isReversed: false);
            });

            Invoke(action);
        }

        /// <summary>
        /// Selects range in the editor view
        /// </summary>
        public static void Select(int startLine, int startColumn, int endLine, int endColumn)
        {
            var action = new Action(() =>
            {
                var selection = CoreEditor.View.Selection;
                var snapshot = CoreEditor.View.TextBuffer.CurrentSnapshot;

                var line1 = snapshot.GetLineFromLineNumber(startLine);
                var start = line1.Start + startColumn;

                var line2 = snapshot.GetLineFromLineNumber(endLine);
                var end = line2.Start + endColumn;

                selection.Select(new SnapshotSpan(snapshot, start, end - start), isReversed: false);
            });

            Invoke(action);
        }
    }
}
