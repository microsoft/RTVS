// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;

namespace Microsoft.Common.Core.Shell {
    /// <summary>
    /// Basic shell provides access to services such as 
    /// composition container, export provider, global VS IDE
    /// services and so on.
    /// </summary>
    public interface ICoreShell: ICompositionCatalog {
        /// <summary>
        /// Retrieves global service from the host application.
        /// This method is not thread safe and should not be called 
        /// from async methods.
        /// </summary>
        /// <typeparam name="T">Service interface type such as IVsUIShell</typeparam>
        /// <param name="type">Service type if different from T, such as typeof(SVSUiShell)</param>
        /// <returns>Service instance of null if not found.</returns>
        T GetGlobalService<T>(Type type = null) where T : class;

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
        void DispatchOnUIThread(Action action);

        /// <summary>
        /// Provides access to the application main thread, so users can know if the task they are trying
        /// to execute is executing from the right thread.
        /// </summary>
        Thread MainThread { get; set; }

        /// <summary>
        /// Fires when host application enters idle state.
        /// </summary>
        event EventHandler<EventArgs> Idle;

        /// <summary>
        /// Fires when host application is terminating
        /// </summary>
        event EventHandler<EventArgs> Terminating;

        /// <summary>
        /// Displays error message in a host-specific UI
        /// </summary>
        void ShowErrorMessage(string message);

        /// <summary>
        /// Shows the context menu with the specified command ID at the specified location
        /// </summary>
        /// <param name="contextMenuGroup"></param>
        /// <param name="contextMenuId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void ShowContextMenu(Guid contextMenuGroup, int contextMenuId, int x, int y);

        /// <summary>
        /// Displays message with specified buttons in a host-specific UI
        /// </summary>
        MessageButtons ShowMessage(string message, MessageButtons buttons);

        /// <summary>
        /// Returns host locale ID
        /// </summary>
        int LocaleId { get; }

        /// <summary>
        /// <summary>
        /// Tells if code runs in unit test environment
        /// </summary>
        bool IsUnitTestEnvironment { get; set; }

        /// <summary>
        /// Tells if code runs in UI test environment
        /// </summary>
        bool IsUITestEnvironment { get; set; }

        /// <summary>
        /// Forces idle time processing
        /// </summary>
        void DoIdle();
    }
}
