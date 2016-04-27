// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Common.Core;
using Microsoft.R.Debugger;
using Microsoft.R.Editor.Data;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.DataInspect.Office;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Model for variable tree grid, that provides UI customization of <see cref="DebugEvaluationResult"/>
    /// </summary>
    public sealed class VariableViewModel : RSessionDataObject, IIndexedItem {
        private IObjectDetailsViewer _detailsViewer;
        private string _title;

        public VariableViewModel() { Index = -1; }

        /// <summary>
        /// Create new instance of <see cref="VariableViewModel"/>
        /// </summary>
        /// <param name="evaluation">R session's evaluation result</param>
        /// <param name="truncateChildren">true to truncate children returned by GetChildrenAsync</param>
        public VariableViewModel(DebugEvaluationResult evaluation, int index = -1, int? maxChildrenCount = null) :
            base(evaluation, maxChildrenCount) {

            Index = index;
            var result = DebugEvaluation as DebugValueEvaluationResult;
            if (result != null) {
                SetViewButtonStatus(result);
            }
        }

        private void SetViewButtonStatus(DebugValueEvaluationResult result) {
            var aggregator = VsAppShell.Current.ExportProvider.GetExportedValue<IObjectDetailsViewerAggregator>();
            _detailsViewer = aggregator.GetViewer(result);
            _title = result.Name;

            CanShowDetail = _detailsViewer != null;
            if (CanShowDetail) {
                ShowDetailCommand = new DelegateCommand(async (o) => await _detailsViewer.ViewAsync(result, _title), (o) => CanShowDetail);
                ShowDetailCommandTooltip = Resources.ShowDetailCommandTooltip;
            }

            var tableCaps = (ViewerCapabilities.Table | ViewerCapabilities.List);
            CanShowOpenCsv = CanShowDetail && (_detailsViewer.Capabilities & tableCaps) != 0 && result.Length > 0;
            if (CanShowOpenCsv) {
                OpenInCsvAppCommand = new DelegateCommand(OpenInCsvApp, (o) => CanShowOpenCsv);
                OpenCsvAppCommandTooltip = Resources.OpenCsvAppCommandTooltip;
            }
        }

        #region Ellipsis 

        private static Lazy<VariableViewModel> _ellipsis = Lazy.Create(() => {
            var instance = new VariableViewModel();
            instance.Name = string.Empty;
            instance.Value = Resources.VariableExplorer_Truncated;
            instance.HasChildren = false;
            return instance;
        });

        public static VariableViewModel Ellipsis {
            get { return _ellipsis.Value; }
        }

        #endregion

        protected override async Task<IReadOnlyList<IRSessionDataObject>> GetChildrenAsyncInternal() {
            List<IRSessionDataObject> result = null;

            var valueEvaluation = DebugEvaluation as DebugValueEvaluationResult;
            if (valueEvaluation == null) {
                Debug.Assert(false, $"{nameof(VariableViewModel)} result type is not {typeof(DebugValueEvaluationResult)}");
                return result;
            }

            if (valueEvaluation.HasChildren) {
                await TaskUtilities.SwitchToBackgroundThread();

                const DebugEvaluationResultFields fields =
                    DebugEvaluationResultFields.Expression |
                    DebugEvaluationResultFields.Kind |
                    DebugEvaluationResultFields.ReprStr |
                    DebugEvaluationResultFields.TypeName |
                    DebugEvaluationResultFields.Classes |
                    DebugEvaluationResultFields.Length |
                    DebugEvaluationResultFields.SlotCount |
                    DebugEvaluationResultFields.AttrCount |
                    DebugEvaluationResultFields.Dim |
                    DebugEvaluationResultFields.Flags;
                IReadOnlyList<DebugEvaluationResult> children = await valueEvaluation.GetChildrenAsync(fields, MaxChildrenCount, 100);

                result = new List<IRSessionDataObject>();
                for (int i = 0; i < children.Count; i++) {
                    result.Add(new VariableViewModel(children[i], index: i, maxChildrenCount: GetMaxChildrenCount(children[i])));
                }

                // return children can be less than value's length in some cases e.g. missing parameter
                if (valueEvaluation.Length > result.Count
                    && (valueEvaluation.Length > MaxChildrenCount)) {
                    result.Add(VariableViewModel.Ellipsis); // insert dummy child to indicate truncation in UI
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
        public string OpenCsvAppCommandTooltip { get; private set; }

        private void OpenInCsvApp(object parameter) {
            CsvAppFileIO.OpenDataCsvApp(DebugEvaluation).DoNotWait();
        }
        #endregion
     }
}
