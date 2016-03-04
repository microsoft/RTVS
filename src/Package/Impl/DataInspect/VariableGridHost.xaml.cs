// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Microsoft.R.Debugger;
using Microsoft.VisualStudio.R.Package.DataInspect.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Control that shows two dimensional R object
    /// </summary>
    public partial class VariableGridHost : UserControl {
        private EvaluationWrapper _evaluation;
        private VariableSubscription _subscription;
        private IVariableDataProvider _variableProvider;

        public VariableGridHost() {
            InitializeComponent();

            _variableProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IVariableDataProvider>();
        }
        
        internal void SetEvaluation(EvaluationWrapper evaluation) {
            ClearError();

            VariableGrid.Initialize(new GridDataProvider(evaluation));

            _evaluation = evaluation;

            if (_subscription != null) {
                _variableProvider.Unsubscribe(_subscription);
                _subscription = null;
            }

            _subscription = _variableProvider.Subscribe(
                evaluation.FrameIndex,
                evaluation.Expression,
                SubscribeAction);
        }

        private void SubscribeAction(DebugEvaluationResult evaluation) {
            VsAppShell.Current.DispatchOnUIThread(
                () => {
                    if (evaluation is DebugErrorEvaluationResult) {
                        // evaluation error, this could happen if R object is removed
                        var error = (DebugErrorEvaluationResult)evaluation;
                        SetError(error.ErrorText);
                        return;
                    }

                    var wrapper = new EvaluationWrapper(evaluation);

                    if (wrapper.TypeName == "NULL" && wrapper.Value == "NULL") {
                        // the variable should have been removed
                        SetError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                Package.Resources.VariableGrid_Missing,
                                evaluation.Expression));
                    } else if (wrapper.Dimensions.Count != 2) {
                        // the same evaluation changed to non-matrix
                        SetError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                Package.Resources.VariableGrid_NotTwoDimension,
                                evaluation.Expression));
                    } else if (wrapper.Dimensions[0] != _evaluation.Dimensions[0]
                        || wrapper.Dimensions[1] != _evaluation.Dimensions[1]) {
                        ClearError();

                        // matrix size changed. Reset the evaluation
                        SetEvaluation(wrapper);
                    } else {
                        ClearError();
                        
                        // size stays same. Refresh
                        VariableGrid.Refresh();
                    }
                });
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
