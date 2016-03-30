// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.R.Package.Windows;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    [Guid("3F6855E6-E2DB-46F2-9820-EDC794FE8AFE")]
    internal sealed class VariableGridWindowPane : RToolWindowPane {
        private VariableGridHost _gridHost;

        public VariableGridWindowPane() {
            Caption = Resources.VariableGrid_Caption;
            Content = _gridHost = new VariableGridHost();

            BitmapImageMoniker = KnownMonikers.VariableProperty;
        }

        internal void SetEvaluation(EvaluationWrapper evaluation) {
            if (!string.IsNullOrWhiteSpace(evaluation.Expression)) {
                Caption = Invariant($"{Resources.VariableGrid_Caption}: {evaluation.Expression}");
            }

            _gridHost.SetEvaluation(evaluation);
        }
    }
}
