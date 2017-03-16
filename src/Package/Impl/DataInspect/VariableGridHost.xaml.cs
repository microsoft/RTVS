// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.DataInspection;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.R.Package.Shell;
using static Microsoft.R.DataInspection.REvaluationResultProperties;

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

            _aggregator = VsAppShell.Current.GlobalServices.GetService<IObjectDetailsViewerAggregator>();
            _rSession = VsAppShell.Current.GlobalServices.GetService<IRInteractiveWorkflowProvider>().GetOrCreate().RSession;
            _rSession.Mutated += RSession_Mutated;
        }

        public void CleanUp() {
            _rSession.Mutated -= RSession_Mutated;
        }

        private void RSession_Mutated(object sender, System.EventArgs e) {
            if (_evaluation != null) {
                EvaluateAsync().DoNotWait();
            }
        }

        private async Task EvaluateAsync() {
            try {
                await TaskUtilities.SwitchToBackgroundThread();
                const REvaluationResultProperties properties = ClassesProperty| ExpressionProperty | TypeNameProperty | DimProperty | LengthProperty;

                var result = await _rSession.TryEvaluateAndDescribeAsync(_evaluation.Expression, properties, null);
                var wrapper = new VariableViewModel(result, _aggregator);

                VsAppShell.Current.DispatchOnUIThread(() => SetEvaluation(wrapper));
            } catch (Exception ex) {
                VsAppShell.Current.DispatchOnUIThread(() => SetError(ex.Message));
            }
        }

        internal void SetEvaluation(VariableViewModel wrapper) {
            VsAppShell.Current.AssertIsOnMainThread();

            // Is the variable gone?
            if (wrapper.TypeName == null) {
                SetError(string.Format(CultureInfo.InvariantCulture, Package.Resources.VariableGrid_Missing, wrapper.Expression));
                _evaluation = null;
                return;
            }

            ClearError();

            // Does it have the same size and shape? If so, can update in-place (without losing scrolling etc).
            if (_evaluation?.Dimensions.SequenceEqual(wrapper.Dimensions) == true) {
                VariableGrid.Refresh();
                return;
            }

            // Otherwise, need to refresh the whole thing from scratch.
            VariableGrid.Initialize(new GridDataProvider(wrapper));
            _evaluation = wrapper;
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
