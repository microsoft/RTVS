// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.InteractiveWorkflow {
    public interface IDebuggerModeTracker {
        bool IsInBreakMode { get; }

        /// <summary>
        /// If true, the application is in debug mode
        /// </summary>
        bool IsDebugging { get; }

        event EventHandler EnterBreakMode;
        event EventHandler LeaveBreakMode;
    }
}