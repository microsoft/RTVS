using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.R.Debugger;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Shell;
using Newtonsoft.Json.Linq;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class VariableChangedArgs : EventArgs {
        public EvaluationWrapper NewVariable { get; set; }
    }

    internal class VariableProvider {
        #region members and ctor

        private IRSession _rSession;
        private DebugSession _debugSession;

        public VariableProvider() {
            var sessionProvider = VsAppShell.Current.ExportProvider.GetExport<IRSessionProvider>().Value;
            sessionProvider.CurrentChanged += RSessionProvider_CurrentChanged;

            IdleTimeAction.Create(() => {
                SetRSession(sessionProvider.Current).SilenceException<Exception>().DoNotWait();
            }, 10, typeof(VariableProvider));
        }

        #endregion

        #region Public

        private static Lazy<VariableProvider> _instance = new Lazy<VariableProvider>(() => new VariableProvider());
        /// <summary>
        /// Singleton
        /// </summary>
        public static VariableProvider Current => _instance.Value;

        /// <summary>
        /// R current session change triggers this SessionsChanged event
        /// </summary>
        public event EventHandler SessionsChanged;
        public event EventHandler<VariableChangedArgs> VariableChanged;

        public EvaluationWrapper LastEvaluation { get; private set; }

        public async Task<JToken> EvaluateGridDataAsync(string expression, string rows, string columns) {
            await TaskUtilities.SwitchToBackgroundThread();

            using (var evaluation = await _debugSession.RSession.BeginEvaluationAsync(false)) {
                var result = await evaluation.EvaluateAsync($"rtvs:::toJSON(rtvs:::grid.data({expression}, {rows}, {columns}))", REvaluationKind.Json);

                if (result.ParseStatus != RParseStatus.OK || result.Error != null || result.JsonResult == null) {
                    throw new InvalidOperationException($"Grid data evaluation failed:{result}");
                }

                return result.JsonResult;
            }
        }

        public async Task<GridHeader> EvaluateGridHeaderAsync(string expression, string range, bool isRow) {
            await TaskUtilities.SwitchToBackgroundThread();

            using (var evaluation = await _debugSession.RSession.BeginEvaluationAsync(false)) {
                var result = await evaluation.EvaluateAsync($"rtvs:::toJSON(rtvs:::grid.header({expression}, {range}, {isRow.ToString().ToUpperInvariant()}))", REvaluationKind.Normal);
                if (result.ParseStatus != RParseStatus.OK || result.Error != null) {
                    throw new InvalidOperationException($"Grid data evaluation failed:{result}");
                }
                Debug.Assert(result.StringResult != null);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<GridHeader>(result.StringResult);
            }
        }

        #endregion

        #region RSession related event handler

        private void RSession_Mutated(object sender, EventArgs e) {
            RefreshVariableCollection().SilenceException<Exception>().DoNotWait();
        }

        /// <summary>
        /// IRSessionProvider.CurrentSessionChanged handler. When current session changes, this is called
        /// </summary>
        private void RSessionProvider_CurrentChanged(object sender, EventArgs e) {
            var sessionProvider = sender as IRSessionProvider;
            Debug.Assert(sessionProvider != null);

            if (sessionProvider != null) {
                SetRSession(sessionProvider.Current).SilenceException<Exception>().DoNotWait();
            }
        }

        #endregion

        private async Task InitializeData() {
            var debugSessionProvider = VsAppShell.Current.ExportProvider.GetExport<IDebugSessionProvider>().Value;

            _debugSession = await debugSessionProvider.GetDebugSessionAsync(_rSession);

            await RefreshVariableCollection();
        }

        private async Task SetRSession(IRSession session) {
            // cleans up old RSession
            if (_rSession != null) {
                _rSession.Mutated -= RSession_Mutated;
            }

            // set new RSession
            _rSession = session;
            if (_rSession != null) {
                _rSession.Mutated += RSession_Mutated;
                await InitializeData();
            }

            // notify the change
            if (SessionsChanged != null) {
                SessionsChanged(this, EventArgs.Empty);
            }
        }

        private async Task RefreshVariableCollection() {
            if (_debugSession == null) {
                return;
            }

            var stackFrames = await _debugSession.GetStackFramesAsync();

            var globalStackFrame = stackFrames.FirstOrDefault(s => s.IsGlobal);
            if (globalStackFrame != null) {
                DebugEvaluationResult evaluation = await globalStackFrame.EvaluateAsync("environment()", "Global Environment");

                LastEvaluation = new EvaluationWrapper(-1, evaluation, false);  // root level doesn't truncate children and return every variables

                if (VariableChanged != null) {
                    VariableChanged(
                        this,
                    new VariableChangedArgs() { NewVariable = LastEvaluation });
                }
            }
        }
    }
}
