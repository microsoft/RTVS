// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.UnitTests.Core.Mef;

namespace Microsoft.R.Components.Test.Fakes.Trackers {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IDebuggerModeTracker))]
    [Export(typeof(TestDebuggerModeTracker))]
    [PartMetadata(PartMetadataAttributeNames.SkipInEditorTestCompositionCatalog, null)]
    public sealed class TestDebuggerModeTracker : IDebuggerModeTracker {
        public bool IsInBreakMode { get; set; }

        public bool IsDebugging { get; set; }

        public bool IsFocusStolenOnBreak => false;

        public bool IsRDebugger() => true;

#pragma warning disable 67
        public event EventHandler EnterBreakMode;
        public event EventHandler LeaveBreakMode;
    }
}