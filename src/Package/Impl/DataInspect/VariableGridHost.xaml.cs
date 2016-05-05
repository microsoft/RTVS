// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Common.Core;
using Microsoft.R.Components.Extensions;
using Microsoft.R.DataInspection;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Control that shows two dimensional R object
    /// </summary>
    public partial class VariableGridHost : UserControl {
        private readonly IObjectDetailsViewerAggregator _aggregator;
        private readonly IRSession _rSession;
        private VariableViewModel _evaluation;

        public VariableGridHost() {
            InitializeComponent();

            _aggregator = VsAppShell.Current.ExportProvider.GetExportedValue<IObjectDetailsViewerAggregator>();
            _rSession = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>().GetInteractiveWindowRSession();
            _rSession.Mutated += RSession_Mutated;
        }

        private void RSession_Mutated(object sender, System.EventArgs e) {
            if (_evaluation != null) {
                EvaluateAsync().DoNotWait();
            }
        }

        private async Task EvaluateAsync() {
            try {
                await TaskUtilities.SwitchToBackgroundThread();
                const RValueProperties fields = RValueProperties.Classes
                        | RValueProperties.Expression
                        | RValueProperties.TypeName
                        | RValueProperties.Dim
                        | RValueProperties.Length;

                var result = await _rSession.TryEvaluateAndDescribeAsync(_evaluation.Expression, fields, null);
                var wrapper = new VariableViewModel(result, _aggregator);

                VsAppShell.Current.DispatchOnUIThread(() => SetEvaluation(wrapper));
            } catch (Exception ex) {
                VsAppShell.Current.DispatchOnUIThread(() => SetError(ex.Message));
            }
        }

        internal void SetEvaluation(VariableViewModel wrapper) {
            VsAppShell.Current.AssertIsOnMainThread();

            if (wrapper.TypeName == "NULL" && wrapper.Value == "NULL") {
                // the variable should have been removed
                SetError(string.Format(CultureInfo.InvariantCulture, Package.Resources.VariableGrid_Missing, wrapper.Expression));
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
            VsAppShell.Current.AssertIsOnMainThread();

            ErrorTextBlock.Text = text;
            ErrorTextBlock.Visibility = Visibility.Visible;
            VariableGrid.Visibility = Visibility.Collapsed;
        }

        private void ClearError() {
            VsAppShell.Current.AssertIsOnMainThread();

            ErrorTextBlock.Visibility = Visibility.Collapsed;
            VariableGrid.Visibility = Visibility.Visible;
        }
    }
}
