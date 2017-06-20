// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Windows;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using static System.FormattableString;

namespace Microsoft.R.Editor.SuggestedActions.Actions {
    internal sealed class InstallPackageSuggestedAction : LibrarySuggestedAction {
        private const string TelemetryId = "AEB4040B-001C-4D74-B2CF-FD46D6686E1E";

        public InstallPackageSuggestedAction(ITextView textView, ITextBuffer textBuffer, IRInteractiveWorkflow workflow, int position) :
            base(textView, textBuffer, workflow, position, Windows_Resources.SmartTagName_InstallPackage) { }

        protected override string GetCommand(string libraryName) {
            return Invariant($"install.packages('{libraryName}')");
        }

        public override bool TryGetTelemetryId(out Guid telemetryId) {
            telemetryId = new Guid(TelemetryId);
            return true;
        }
    }
}
