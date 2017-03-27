// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.R.Package.Windows;
using Microsoft.VisualStudio.R.Packages.R;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    [Guid(RGuidList.VariableGridWindowGuidString)]
    internal sealed class VariableGridWindowPane : RToolWindowPane {
        private readonly VariableGridHost _gridHost;

        public VariableGridWindowPane() {
            Caption = Resources.VariableGrid_Caption;
            Content = _gridHost = new VariableGridHost();
            BitmapImageMoniker = KnownMonikers.VariableProperty;
        }

        internal void SetEvaluation(VariableViewModel evaluation, string caption) {
            if (!string.IsNullOrWhiteSpace(evaluation.Expression)) {
                Caption = Invariant($"{Resources.VariableGrid_Caption}: {caption}");
            }
            _gridHost.SetEvaluation(evaluation);
        }

        protected override void OnClose() {
            base.OnClose();
            _gridHost.CleanUp();
        }
    }
}
