// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.DataInspection;
using Microsoft.R.Editor.Data;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Shell;
using static Microsoft.R.DataInspection.REvaluationResultProperties;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Control that shows two dimensional R object
    /// </summary>
    public partial class VariableGridHost : IDisposable {
        private const string ViewEnvName = "rtvs:::view_env";

        private readonly IServiceContainer _services;
        private readonly IRSession _rSession;
        private readonly DisposableBag _disposableBag = new DisposableBag(nameof(VariableGridHost));

        private IRSessionDataObject _evaluation;

        public VariableGridHost() : this(VsAppShell.Current.Services) { }

        public VariableGridHost(IServiceContainer services) {
            InitializeComponent();

            _services = services;
            _rSession = _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate().RSession;
            _rSession.Mutated += RSession_Mutated;

            _disposableBag
                .Add(() => _rSession.Mutated -= RSession_Mutated)
                .Add(DeleteCachedVariable);

            FocusManager.SetFocusedElement(this, VariableGrid);
        }

        public void Dispose() => _disposableBag.TryDispose();
        
        private void DeleteCachedVariable() {
            if (_evaluation != null && _evaluation.Expression.StartsWithOrdinal(ViewEnvName)) {
                if (_rSession.IsHostRunning) {
                    var varName = _evaluation.Expression.Substring(ViewEnvName.Length + 1);
                    try {
                        _rSession.ExecuteAsync(Invariant($"rm('{varName}', envir = {ViewEnvName})")).DoNotWait();
                    } catch (Exception ex) when (!ex.IsCriticalException()) { }
                }
            }
            _evaluation = null;
        }

        private void RSession_Mutated(object sender, EventArgs e) {
            if (_evaluation != null && _services.GetService<IRSettings>().GridDynamicEvaluation) {
                EvaluateAsync().DoNotWait();
            }
        }

        private async Task EvaluateAsync() {
            try {
                await TaskUtilities.SwitchToBackgroundThread();
                const REvaluationResultProperties properties = ClassesProperty | ExpressionProperty | TypeNameProperty | DimProperty | LengthProperty;

                var result = await _rSession.TryEvaluateAndDescribeAsync(_evaluation.Expression, properties, null);
                var wrapper = new VariableViewModel(result, _services);

                _services.MainThread().Post(() => SetEvaluation(wrapper));
            } catch (Exception ex) {
                _services.MainThread().Post(() => SetError(ex.Message));
            }
        }

        internal void SetEvaluation(IRSessionDataObject dataObject) {
            _services.MainThread().Assert();

            // Is the variable gone?
            if (dataObject.TypeName == null) {
                SetError(string.Format(CultureInfo.InvariantCulture, Package.Resources.VariableGrid_Missing, dataObject.Expression));
                _evaluation = null;
                return;
            }

            ClearError();

            // Does it have the same size and shape? If so, can update in-place (without losing scrolling etc).
            if (_evaluation?.Dimensions.SequenceEqual(dataObject.Dimensions) == true) {
                VariableGrid.Refresh();
                return;
            }

            // Otherwise, need to refresh the whole thing from scratch.
            var session = _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate().RSession;
            VariableGrid.Initialize(new GridDataProvider(session, dataObject));
            _evaluation = dataObject;
        }

        private void SetError(string text) {
            _services.MainThread().Assert();

            ErrorTextBlock.Text = text;
            ErrorTextBlock.Visibility = Visibility.Visible;
            VariableGrid.Visibility = Visibility.Collapsed;
        }

        private void ClearError() {
            _services.MainThread().Assert();

            ErrorTextBlock.Visibility = Visibility.Collapsed;
            VariableGrid.Visibility = Visibility.Visible;
        }
    }
}
