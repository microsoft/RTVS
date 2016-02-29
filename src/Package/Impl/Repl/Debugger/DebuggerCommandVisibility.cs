// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.VisualStudio.R.Package.Repl.Debugger {
    [Flags]
    enum DebuggerCommandVisibility {
        /// <summary>
        /// Command is visible and available when 
        /// IDE is in default (design) mode
        /// </summary>
        DesignMode,

        /// <summary>
        /// Command is visible and enabled when
        /// in debug mode and code is running
        /// </summary>
        Run = 0x01,

        /// <summary>
        /// Command is visible and enabled when
        /// executing is stopped on a breakpoint
        /// </summary>
        Stopped = 0x02,

        /// <summary>
        /// Command is available in debug mode
        /// </summary>
        DebugMode = Run | Stopped,
    }
}
