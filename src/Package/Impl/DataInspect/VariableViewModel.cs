// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.DataInspection;
using Microsoft.R.Editor.Data;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.DataInspect.Office;
using Microsoft.VisualStudio.R.Package.Shell;
using static System.FormattableString;
using static Microsoft.R.DataInspection.REvaluationResultProperties;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Model for variable tree grid, that provides UI customization of <see cref="REvaluationInfo"/>
    /// </summary>
    public sealed class VariableViewModel : RSessionDataObject, IIndexedItem {
        private readonly IObjectDetailsViewerAggregator _aggregator;
        private IObjectDetailsViewer _detailsViewer;
        private readonly IServiceContainer _services;
        private readonly IRSettings _settings;
        private string _title;
        private bool _deleted;

        public VariableViewModel() { Index = -1; }

        public VariableViewModel(IREvaluationResultInfo evaluation, IServiceContainer services, int index = -1, int? maxChildrenCount = null) :
            base(evaluation, services.GetService<IRSettings>().EvaluateActiveBindings, maxChildrenCount) {
            _services = services;
            _aggregator = services.GetService<IObjectDetailsViewerAggregator>();
            _settings = services.GetService<IRSettings>();
            Index = index;
            var result = DebugEvaluation as IRValueInfo;
            if (result != null) {
                SetViewButtonStatus(result);
            }
        }

        private void SetViewButtonStatus(IRValueInfo result) {
            _detailsViewer = _aggregator.GetViewer(result);
            _title = result.Name;

            CanShowDetail = _detailsViewer != null;
            if (CanShowDetail) {
                ShowDetailCommand = new DelegateCommand(o => _detailsViewer.ViewAsync(result.Expression, _title).DoNotWait(), o => CanShowDetail);
                ShowDetailCommandTooltip = Resources.ShowDetailCommandTooltip;
            }

            CanShowOpenCsv = result.CanCoerceToDataFrame && (result.Length > 1 || result.TypeName == "S4");
            if (CanShowOpenCsv) {
                OpenInCsvAppCommand = new DelegateCommand(OpenInCsvApp, o => CanShowOpenCsv);
                OpenInCsvAppCommandTooltip = Resources.OpenCsvAppCommandTooltip;
            }
        }

        private static readonly Lazy<VariableViewModel> _ellipsis = Lazy.Create(() => new VariableViewModel {
            Name = string.Empty,
            Value = Resources.VariableExplorer_Truncated,
            HasChildren = false
        });

        public static VariableViewModel Ellipsis => _ellipsis.Value;

        public static VariableViewModel Error(string text) => new VariableViewModel {
            Name = string.Empty,
            Value = text,
            HasChildren = false
        };

        private static readonly string Repr = RValueRepresentations.Str(100);

        protected override async Task<IReadOnlyList<IRSessionDataObject>> GetChildrenAsyncInternal() {
            List<IRSessionDataObject> result = null;

            var valueEvaluation = DebugEvaluation as IRValueInfo;
            if (valueEvaluation == null) {
                Debug.Assert(false, $"{nameof(VariableViewModel)} result type is not {typeof(IRValueInfo)}");
                return result;
            }

            if (valueEvaluation.HasChildren) {
                await TaskUtilities.SwitchToBackgroundThread();

                REvaluationResultProperties properties =
                    ExpressionProperty |
                    AccessorKindProperty |
                    TypeNameProperty |
                    ClassesProperty |
                    LengthProperty |
                    SlotCountProperty |
                    AttributeCountProperty |
                    DimProperty |
                    FlagsProperty |
                    CanCoerceToDataFrameProperty |
                    (_settings.EvaluateActiveBindings ? ComputedValueProperty : 0);
                IReadOnlyList<IREvaluationResultInfo> children = await valueEvaluation.DescribeChildrenAsync(properties, Repr, MaxChildrenCount);

                result = new List<IRSessionDataObject>();
                for (int i = 0; i < children.Count; i++) {
                    result.Add(new VariableViewModel(children[i], _services, i, GetMaxChildrenCount(children[i])));
                }

                // return children can be less than value's length in some cases e.g. missing parameter
                if (valueEvaluation.Length > result.Count
                    && (valueEvaluation.Length > MaxChildrenCount)) {
                    result.Add(Ellipsis); // insert dummy child to indicate truncation in UI
                }
            }

            return result;
        }

        #region IIndexedItem support

        /// <summary>
        /// Index returned from the evaluation provider. 
        /// DebugEvaluationResult returns in ascending order
        /// </summary>
        public int Index { get; }

        #endregion

        #region Variable Grid command
        public bool CanShowDetail { get; private set; }

        public ICommand ShowDetailCommand { get; private set; }
        public string ShowDetailCommandTooltip { get; private set; }

        public bool CanShowOpenCsv { get; private set; }

        public ICommand OpenInCsvAppCommand { get; private set; }
        public string OpenInCsvAppCommandTooltip { get; private set; }

        private void OpenInCsvApp(object parameter) {
            CsvAppFileIO.OpenDataCsvApp(DebugEvaluation, VsAppShell.Current, VsAppShell.Current.FileSystem(), new ProcessServices()).DoNotWait();
        }

        /// <summary>
        /// Deletes variable represented by this mode
        /// </summary>
        public async Task DeleteAsync(string envExpr) {
            if (!_deleted) {
                _deleted = true;
                var workflow = _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
                var session = workflow.RSession;
                try {
                    using (var e = await session.BeginInteractionAsync(isVisible: false)) {
                        string rmText = string.IsNullOrWhiteSpace(envExpr) ? Invariant($"rm({Name})") : Invariant($"rm({Name}, envir = {envExpr})");
                        await e.RespondAsync(rmText);
                    }
                } catch (RException rex) {
                    VsAppShell.Current.ShowErrorMessage(Resources.Error_UnableToDeleteVariable.FormatInvariant(rex.Message));
                } catch (RHostDisconnectedException) {
                }
            }
        }
        #endregion
    }
}
