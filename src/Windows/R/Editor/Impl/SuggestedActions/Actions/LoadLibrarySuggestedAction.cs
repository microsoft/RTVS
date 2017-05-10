// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Windows;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using static System.FormattableString;

namespace Microsoft.R.Editor.SuggestedActions.Actions {
    internal sealed class LoadLibrarySuggestedAction : LibrarySuggestedAction {
        private const string TelemetryId = "6812FCEB-DFC8-4DE5-954E-8641FBFD6DC4";

        public LoadLibrarySuggestedAction(ITextView textView, ITextBuffer textBuffer, IRInteractiveWorkflow workflow, int position) :
            base(textView, textBuffer, workflow, position, Windows_Resources.SmartTagName_LoadLibrary) { }

        protected override string GetCommand(string libraryName) {
            return Invariant($"library({libraryName})");
        }

        public override bool TryGetTelemetryId(out Guid telemetryId) {
            telemetryId = new Guid(TelemetryId);
            return true;
        }
    }
}
