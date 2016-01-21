using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.R.Debugger;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.DataInspect.Definitions;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class VariableProvider: IVariableDataProvider {
        #region Members and ctor
        private IRSession _rSession;
        private IDebugSessionProvider _debugSessionProvider;
        private DebugSession _debugSession;

        public VariableProvider() {
            var sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            _rSession = sessionProvider.GetInteractiveWindowRSession();
            _rSession.Mutated += RSession_Mutated;

            _debugSessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IDebugSessionProvider>();
            IdleTimeAction.Create(() => {
                RefreshVariableCollection().SilenceException<Exception>().DoNotWait();
            }, 10, typeof(VariableProvider));
        }
        #endregion

        #region Public

        private static Lazy<VariableProvider> _instance = Lazy.Create(() => new VariableProvider());
        /// <summary>
        /// Singleton
        /// </summary>
        public static IVariableDataProvider Current => _instance.Value;

        #region IVariableDataProvider
        public event EventHandler<VariableChangedArgs> VariableChanged;
        public EvaluationWrapper LastEvaluation { get; private set; }

        public async Task<IGridData<string>> GetGridDataAsync(string expression, GridRange gridRange) {
            await TaskUtilities.SwitchToBackgroundThread();

            var rSession = _rSession;

            string rows = RangeToRString(gridRange.Rows);
            string columns = RangeToRString(gridRange.Columns);

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

        public void Dispose() {
            // Only used in tests to make sure each instance 
            // of the variable explorer uses fresh variable provider
            _rSession.Mutated -= RSession_Mutated;
            _rSession = null;
            _instance = null;
        }
        #endregion

        #region RSession related event handler

        private void RSession_Mutated(object sender, EventArgs e) {
            RefreshVariableCollection().SilenceException<Exception>().DoNotWait();
        }

        #endregion

        private async Task RefreshVariableCollection() {
            if (_rSession != null) {
                if (_debugSession == null) {
                    _debugSession = await _debugSessionProvider.GetDebugSessionAsync(_rSession);
                }

                var stackFrames = await _debugSession.GetStackFramesAsync();
                var globalStackFrame = stackFrames.FirstOrDefault(s => s.IsGlobal);
                if (globalStackFrame != null) {
                    DebugEvaluationResult evaluation = await globalStackFrame.EvaluateAsync("environment()", "Global Environment");

                    LastEvaluation = new EvaluationWrapper(-1, evaluation, false);  // root level doesn't truncate children and return every variables
                    VariableChanged?.Invoke(this, new VariableChangedArgs() { NewVariable = LastEvaluation });
                }
            }
        }

        private static string RangeToRString(Range range) {
            return $"{range.Start + 1}:{range.Start + range.Count}";
        }
    }
}
