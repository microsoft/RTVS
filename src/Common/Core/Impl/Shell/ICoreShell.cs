// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;

namespace Microsoft.Common.Core.Shell {
    /// <summary>
    /// Basic shell provides access to services such as 
    /// composition container, export provider, global VS IDE
    /// services and so on.
    /// </summary>
    public interface ICoreShell: IIdleTimeService {
        /// <summary>
        /// Application name to use in log, system events, etc.
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        /// Application locale ID (LCID)
        /// </summary>
        int LocaleId { get; }

        /// <summary>
        /// Application-global services access
        /// </summary>
        IServiceContainer Services { get; }

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
        /// Fires when host application has completed it's startup sequence
        /// </summary>
        event EventHandler<EventArgs> Started;

        /// <summary>
        /// Fires when host application is terminating
        /// </summary>
        event EventHandler<EventArgs> Terminating;

        /// <summary>
        /// Tells if code runs in unit test environment
        /// </summary>
        bool IsUnitTestEnvironment { get; }
    }
}
