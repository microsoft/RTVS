using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Undo;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Shell {
    /// <summary>
    /// Host for Web editing component. This interface provides 
    /// application-specific services and settings.
    /// </summary>
    public interface IEditorShell {
        /// <summary>
        /// Application composition service
        /// </summary>
        ICompositionService CompositionService { get; }

        /// <summary>
        /// Host export provider
        /// </summary>
        ExportProvider ExportProvider { get; }

        /// <summary>
        /// Provides shim that implements ICommandTarget over 
        /// application-specific command target. For example, 
        /// Visual Studio is using IOleCommandTarget.
        /// </summary>
        /// <param name="commandTarget">Command target</param>
        /// <returns>Web components compatible command target</returns>
        ICommandTarget TranslateCommandTarget(ITextView textView, object commandTarget);

        /// <summary>
        /// Provides application-specific command target.
        /// For example, Visual Studio is using IOleCommandTarget.
        /// </summary>
        /// <param name="commandTarget">Command target</param>
        /// <returns>Host compatible command target</returns>
        object TranslateToHostCommandTarget(ITextView textView, object commandTarget);

        /// <summary>
        /// Provides a way to execute action on UI thread while
        /// UI thread is waiting for the completion of the action.
        /// May be implemented using ThreadHelper in VS or via
        /// SynchronizationContext in all-managed application.
        /// 
        /// This can be blocking or non blocking dispatch, preferrably
        /// non blocking
        /// </summary>
        /// <param name="action">Action to execute</param>
        void DispatchOnUIThread(Action action, DispatcherPriority priority);

        /// <summary>
        /// Provides access to the application main thread, so users can know if the task they are trying
        /// to execute is executing from the right thread.
        /// </summary>
        Thread MainThread { get; }

        /// <summary>
        /// Fires when host application enters idle state.
        /// </summary>
        event EventHandler<EventArgs> Idle;

        /// <summary>
        /// Fires when host application is terminating
        /// </summary>
        event EventHandler<EventArgs> Terminating;

        /// <summary>
        /// Creates compound undo action
        /// </summary>
        /// <param name="textView">Text view</param>
        /// <param name="textBuffer">Text buffer</param>
        /// <returns>Undo action instance</returns>
        ICompoundUndoAction CreateCompoundAction(ITextView textView, ITextBuffer textBuffer);

        /// <summary>
        /// Displays error message in a host-specific UI
        /// </summary>
        void ShowErrorMessage(string message);

        /// <summary>
        /// Displays message with specified buttons in a host-specific UI
        /// </summary>
        MessageButtons ShowMessage(string message, MessageButtons buttons);

        string BrowseForFileOpen(IntPtr owner, string filter, string initialPath = null, string title = null);

        string BrowseForFileSave(IntPtr owner, string filter, string initialPath = null, string title = null);

        /// <summary>
        /// Displays help on the specified topic
        /// </summary>
        bool ShowHelp(string topicName);

        /// <summary>
        /// Returns host locale ID
        /// </summary>
        int LocaleId { get; }

        /// <summary>
        /// Returns path to application-specific user folder, such as VisualStudio\11.0
        /// </summary>
        string UserFolder { get; }

        /// <summary>
        /// Host service provider (can be null).
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Tells if code runs in unit test environment
        /// </summary>
        bool IsUnitTestEnvironment { get; }

        /// <summary>
        /// Tells if code runs in UI test environment
        /// </summary>
        bool IsUITestEnvironment { get; }
    }
}
