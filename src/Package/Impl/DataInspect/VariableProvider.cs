using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.R.Debugger;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class VariableProvider: IDisposable {
        #region members and ctor

        private IRSession _rSession;
        private DebugSession _debugSession;

        public VariableProvider() {
            var sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            _rSession = sessionProvider.GetInteractiveWindowRSession();
            _rSession.Mutated += RSession_Mutated;
            IdleTimeAction.Create(() => {
                InitializeData().SilenceException<Exception>().DoNotWait();
            }, 10, typeof(VariableProvider));
        }

        #endregion

        #region Public

        private static VariableProvider _instance;
        /// <summary>
        /// Singleton
        /// </summary>
        public static VariableProvider Current => _instance ?? (_instance = new VariableProvider());

        public event EventHandler<VariableChangedArgs> VariableChanged;

        public EvaluationWrapper GlobalEnvironment { get; private set; }

        public async Task<IGridData<string>> GetGridDataAsync(string expression, GridRange gridRange) {
            await TaskUtilities.SwitchToBackgroundThread();

            var rSession = _rSession;

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
            PublishAllAsync().SilenceException<Exception>().DoNotWait();
        }

        #endregion
        private async Task InitializeData() {
            var debugSessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IDebugSessionProvider>();

            _debugSession = await debugSessionProvider.GetDebugSessionAsync(_rSession);

            await RefreshVariableCollection();
        }

        private async Task RefreshVariableCollection() {
            if (_debugSession == null) {
                return;
            }

            var stackFrames = await _debugSession.GetStackFramesAsync();

            var globalStackFrame = stackFrames.FirstOrDefault(s => s.IsGlobal);
            if (globalStackFrame != null) {
                DebugEvaluationResult evaluation = await globalStackFrame.EvaluateAsync("environment()", "Global Environment");

                GlobalEnvironment = new EvaluationWrapper(-1, evaluation, false);  // root level doesn't truncate children and return every variables

                if (VariableChanged != null) {
                    VariableChanged(
                        this,
                    new VariableChangedArgs() { NewVariable = GlobalEnvironment });
                }
            }
        }

        #region variable subscription model

        private readonly Dictionary<VariableSubscriptionToken, List<VariableSubscription>> _subscribers = new Dictionary<VariableSubscriptionToken, List<VariableSubscription>>();

        public string GlobalEnvironmentExpression {
            get {
                return "sys.frame(0)";
            }
        }

        public VariableSubscription Subscribe(
            string environmentExpression,
            string variableExpression,
            Action<DebugEvaluationResult> executeAction) {

            var token = new VariableSubscriptionToken(environmentExpression, variableExpression);

            var subscription = new VariableSubscription(
                token,
                executeAction,
                Unsubscribe);

            lock (_subscribers) {
                List<VariableSubscription> subscriptions;
                if (_subscribers.TryGetValue(subscription.Token, out subscriptions)) {
                    subscriptions.Add(subscription);
                } else {
                    _subscribers.Add(
                        token,
                        new List<VariableSubscription>() { subscription });
                }
            }

            return subscription;
        }

        public void Unsubscribe(VariableSubscription subscription) {
            lock (_subscribers) {
                List<VariableSubscription> subscriptions;
                if (_subscribers.TryGetValue(subscription.Token, out subscriptions)) {
                    if (!subscriptions.Remove(subscription)) {
                        Debug.Fail("Subscription is not found");
                    }
                    if (subscriptions.Count == 0) {
                        _subscribers.Remove(subscription.Token);
                    }
                }
            }
        }

        private async Task PublishAllAsync() {
            List<Task> subsribeTasks = new List<Task>();
            lock (_subscribers) {
                foreach (var kv in _subscribers) {
                    subsribeTasks.Add(PublishAsync(kv.Key, kv.Value));
                }
            }

            await Task.WhenAll(subsribeTasks);
        }

        private async Task PublishAsync(VariableSubscriptionToken token, IList<VariableSubscription> subscriptions) {
            if (subscriptions.Count == 0) {
                return;
            }

            if (_debugSession == null) {
                return;
            }

            var result = await _debugSession.EvaluateAsync(null, token.Expression, "Global Environment" /* BUG BUG */, token.Environment, DebugEvaluationResultFields.All);

            foreach (var sub in subscriptions) {
                var action = sub.GetExecuteAction();
                action(result);
            }
        }

        #endregion
    }
}
