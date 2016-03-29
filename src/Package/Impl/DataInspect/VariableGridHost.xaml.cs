// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Debugger;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Control that shows two dimensional R object
    /// </summary>
    public partial class VariableGridHost : UserControl {
        private EvaluationWrapper _evaluation;
        private VariableSubscription _subscription;
        private IRSession _rSession;

        public VariableGridHost() {
            InitializeComponent();

            _rSession = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate().RSession;
            _rSession.Mutated += RSession_Mutated;
        }

        private void RSession_Mutated(object sender, System.EventArgs e) {
            EvaluateAsync().DoNotWait();
        }

        private async Task EvaluateAsync() {
            try {
                DebugEvaluationResult result = await Evaluate1Async();

                var wrapper = new EvaluationWrapper(result);

                VsAppShell.Current.DispatchOnUIThread(() => SetEvaluation(wrapper));
            } catch (Exception ex) {
                SetError(ex.Message);
            }
        }

        private async Task<DebugEvaluationResult> Evaluate1Async() {
            await TaskUtilities.SwitchToBackgroundThread();

            var debugSession = await VsAppShell.Current.ExportProvider.GetExportedValue<IDebugSessionProvider>().GetDebugSessionAsync(_rSession);

            const DebugEvaluationResultFields fields = DebugEvaluationResultFields.Classes
                    | DebugEvaluationResultFields.Expression
                    | DebugEvaluationResultFields.TypeName
                    | (DebugEvaluationResultFields.Repr | DebugEvaluationResultFields.ReprStr)
                    | DebugEvaluationResultFields.Dim
                    | DebugEvaluationResultFields.Length;

            return await debugSession.EvaluateAsync(_evaluation.Expression, fields);
        }

        internal void SetEvaluation(EvaluationWrapper wrapper) {
            if (wrapper.TypeName == "NULL" && wrapper.Value == "NULL") {
                // the variable should have been removed
                SetError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Package.Resources.VariableGrid_Missing,
                        wrapper.Expression));
            } else if (wrapper.Dimensions.Count != 2) {
                // the same evaluation changed to non-matrix
                SetError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Package.Resources.VariableGrid_NotTwoDimension,
                        wrapper.Expression));
            } else if (_evaluation == null
                || (wrapper.Dimensions[0] != _evaluation.Dimensions[0] || wrapper.Dimensions[1] != _evaluation.Dimensions[1])) {
                // matrix size changed. Reset the evaluation
                ClearError();

                VariableGrid.Initialize(new GridDataProvider(wrapper));

                _evaluation = wrapper;
            } else {
                ClearError();

                // size stays same. Refresh
                VariableGrid.Refresh();
            }
        }

        private void SetError(string text) {
            ErrorTextBlock.Text = text;
            ErrorTextBlock.Visibility = Visibility.Visible;

            VariableGrid.Visibility = Visibility.Collapsed;
        }

        private void ClearError() {
            ErrorTextBlock.Visibility = Visibility.Collapsed;

            VariableGrid.Visibility = Visibility.Visible;
        }
    }
}
