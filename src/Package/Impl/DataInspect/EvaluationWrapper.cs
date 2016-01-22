using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Common.Core;
using Microsoft.R.Debugger;
using Microsoft.R.Editor.Data;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Package.Repl;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Model for variable tree grid, that provides UI customization of <see cref="DebugEvaluationResult"/>
    /// </summary>
    internal sealed class EvaluationWrapper : RSessionDataObject, IIndexedItem {
        private readonly bool _truncateChildren;

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

                var fields = (DebugEvaluationResultFields.All & ~DebugEvaluationResultFields.ReprAll) |
                        DebugEvaluationResultFields.Repr | DebugEvaluationResultFields.ReprStr;

                // assumption: DebugEvaluationResult returns children in ascending order
                IReadOnlyList<DebugEvaluationResult> children = await valueEvaluation.GetChildrenAsync(fields, MaxChildrenCount, 100);

                result = new List<IRSessionDataObject>();
                for (int i = 0; i < children.Count; i++) {
                    result.Add(new EvaluationWrapper(children[i], index: i, maxChildrenCount: DefaultMaxGrandChildren));
                }

                if (valueEvaluation.Length > result.Count) {
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

        private void ShowVariableGridWindowPane(object parameter) {
            VariableGridWindowPane pane = ToolWindowUtilities.ShowWindowPane<VariableGridWindowPane>(0, true);
            pane.SetEvaluation(this);
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

        public async Task<IGridData<string>> GetGridDataAsync(string expression, GridRange gridRange) {
            await TaskUtilities.SwitchToBackgroundThread();

            var rSession = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>().GetInteractiveWindowRSession();
            if (rSession == null) {
                throw new InvalidOperationException(Invariant($"{nameof(IRSessionProvider)} failed to return RSession for {nameof(EvaluationWrapper)}"));
            }

            string rows = gridRange.Rows.ToRString();
            string columns = gridRange.Columns.ToRString();

            using (var elapsed = new Elapsed("Data:Evaluate:")) {
                using (var evaluator = await rSession.BeginEvaluationAsync(false)) {
                    var result = await evaluator.EvaluateAsync($"rtvs:::grid.dput(rtvs:::grid.data({expression}, {rows}, {columns}))", REvaluationKind.Normal);

                    if (result.ParseStatus != RParseStatus.OK || result.Error != null) {
                        throw new InvalidOperationException($"Grid data evaluation failed:{result}");
                    }

                    var data = GridParser.Parse(result.StringResult);
                    data.Range = gridRange;

                    if (data.ValidHeaderNames
                        && (data.ColumnNames.Count != gridRange.Columns.Count
                            || data.RowNames.Count != gridRange.Rows.Count)) {
                        throw new InvalidOperationException("Header names lengths are different from data's length");
                    }

                    return data;
                }
            }
        }

        #endregion
    }
}
