// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.LanguageServer.Threading {
    /// <summary>
    /// Defines priority of main thread work items
    /// </summary>
    internal enum ThreadPostPriority {
        /// <summary>
        /// Executed once on idle. Typical for idle time timer requests
        /// where subsequent requests replace the existing one in the queue.
        /// </summary>
        IdleOnce,
        
        /// <summary>
        /// Execute when application is idle
        /// </summary>
        Idle,

        /// <summary>
        /// Background priority. Typical for background actions that 
        /// need to be completed on UI thread.
        /// </summary>
        Background,

        /// <summary>
        /// Normal priority, typical user-initiated actions such as typing
        /// </summary>
        Normal
    }
}
