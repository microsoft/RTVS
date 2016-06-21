// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Test.Fakes.Trackers {
    [Export(typeof(IDebuggerModeTracker))]
    [Export(typeof(TestDebuggerModeTracker))]
    public sealed class TestDebuggerModeTracker : IDebuggerModeTracker {
        public bool IsInBreakMode { get; set; }

        public bool IsDebugging { get; set; }

#pragma warning disable 67
        public event EventHandler EnterBreakMode;
        public event EventHandler LeaveBreakMode;
    }
}