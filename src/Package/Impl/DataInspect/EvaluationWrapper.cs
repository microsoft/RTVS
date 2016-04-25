// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Common.Core;
using Microsoft.R.Debugger;
using Microsoft.R.Editor.Data;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.DataInspect.Office;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Model for variable tree grid, that provides UI customization of <see cref="DebugEvaluationResult"/>
    /// </summary>
    public sealed class EvaluationWrapper : RSessionDataObject, IIndexedItem {

        public EvaluationWrapper() { Index = -1; }

        /// <summary>
        /// Create new instance of <see cref="EvaluationWrapper"/>
        /// </summary>
        /// <param name="evaluation">R session's evaluation result</param>
        /// <param name="truncateChildren">true to truncate children returned by GetChildrenAsync</param>
        public EvaluationWrapper(DebugEvaluationResult evaluation, int index = -1, int? maxChildrenCount = null) :
            base(evaluation, maxChildrenCount) {

            Index = index;

            CanShowDetail = ComputeDetailAvailability(DebugEvaluation as DebugValueEvaluationResult);
            if (CanShowDetail) {
                ShowDetailCommand = new DelegateCommand(ShowVariableGridWindowPane, (o) => CanShowDetail);
                ShowDetailCommandTooltip = Resources.ShowDetailCommandTooltip;
            }

            CanShowOpenCsv = ComputeCsvAvailability(DebugEvaluation as DebugValueEvaluationResult);
            if (CanShowOpenCsv) {
                OpenInCsvAppCommand = new DelegateCommand(OpenInCsvApp, (o) => CanShowOpenCsv);
                OpenCsvAppCommandTooltip = Resources.OpenCsvAppCommandTooltip;
            }
        }

        #region Ellipsis 

        private static Lazy<EvaluationWrapper> _ellipsis = Lazy.Create(() => {
            var instance = new EvaluationWrapper();
            instance.Name = string.Empty;
            instance.Value = Resources.VariableExplorer_Truncated;
            instance.HasChildren = false;
            return instance;
        });

        public static EvaluationWrapper Ellipsis {
            get { return _ellipsis.Value; }
        }

        #endregion

        public int FrameIndex {
            get {
                if (base.DebugEvaluation != null && base.DebugEvaluation.StackFrame != null) {
                    return base.DebugEvaluation.StackFrame.Index;
                }

                Debug.Fail("DebugEvaluationResult doesn't set StackFrame");
                return 0;   // global frame index, by default
            }
        }

        protected override async Task<IReadOnlyList<IRSessionDataObject>> GetChildrenAsyncInternal() {
            List<IRSessionDataObject> result = null;

            var valueEvaluation = DebugEvaluation as DebugValueEvaluationResult;
            if (valueEvaluation == null) {
                Debug.Assert(false, $"{nameof(EvaluationWrapper)} result type is not {typeof(DebugValueEvaluationResult)}");
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
                    result.Add(new EvaluationWrapper(children[i], index: i, maxChildrenCount: GetMaxChildrenCount(children[i])));
                }

                // return children can be less than value's length in some cases e.g. missing parameter
                if (valueEvaluation.Length > result.Count
                    && (valueEvaluation.Length > MaxChildrenCount)) {
                    result.Add(EvaluationWrapper.Ellipsis); // insert dummy child to indicate truncation in UI
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

        public bool CanShowDetail { get; }

        public ICommand ShowDetailCommand { get; }
        public string ShowDetailCommandTooltip { get; }

        public bool CanShowOpenCsv { get; }

        public ICommand OpenInCsvAppCommand { get; }
        public string OpenCsvAppCommandTooltip { get; }

        private void ShowVariableGridWindowPane(object parameter) {
            VariableGridWindowPane pane = ToolWindowUtilities.ShowWindowPane<VariableGridWindowPane>(0, true);
            pane.SetEvaluation(this);
        }

        private void OpenInCsvApp(object parameter) {
            CsvAppFileIO.OpenDataCsvApp(DebugEvaluation).DoNotWait();
        }

        private static string[] detailClasses = new string[] { "matrix", "data.frame", "table" };
        private bool ComputeDetailAvailability(DebugValueEvaluationResult evaluation) {
            if (evaluation != null && evaluation.Classes.Any(t => detailClasses.Contains(t))) {
                if (evaluation.Dim != null && evaluation.Dim.Count == 2) {
                    return true;
                }
            }
            return false;
        }
        #endregion

        private bool ComputeCsvAvailability(DebugValueEvaluationResult evaluation) {
            bool result = false;
            if (evaluation != null) {
                result = ComputeDetailAvailability(evaluation);
                if (!result) {
                    result = evaluation.Length > 1;
                }
            }
            return result;
        }
    }
}
