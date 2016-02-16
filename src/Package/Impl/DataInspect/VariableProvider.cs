using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.R.Debugger;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.DataInspect.Definitions;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    [Export(typeof(IVariableDataProvider))]
    internal class VariableProvider: IVariableDataProvider, IDisposable {
        #region members and ctor

        private DebugSession _debugSession;

        [ImportingConstructor]
        public VariableProvider(
            [Import(typeof(IRSessionProvider))]IRSessionProvider sessionProvider,
            [Import(typeof(IDebugSessionProvider))] IDebugSessionProvider debugSessionProvider) {
            if (sessionProvider == null) {
                throw new ArgumentNullException(nameof(sessionProvider));
            }
            if (debugSessionProvider == null) {
                throw new ArgumentNullException(nameof(debugSessionProvider));
            }


            RSession = sessionProvider.GetInteractiveWindowRSession();
            if (RSession == null) {
                throw new InvalidOperationException(Invariant($"{nameof(IRSessionProvider)} failed to return RSession for {nameof(IVariableDataProvider)}"));
            }
            RSession.Mutated += RSession_Mutated;

            IdleTimeAction.Create(() => {
                PublishAllAsync().SilenceException<Exception>().DoNotWait();
            }, 10, typeof(VariableProvider));
        }

        #endregion

        #region Public

        public const string GlobalEnvironmentExpression = "base::.GlobalEnv";

        public IRSession RSession { get; }

        public void Dispose() {
            RSession.Mutated -= RSession_Mutated;
        }
        #endregion

        #region RSession related event handler

        private void RSession_Mutated(object sender, EventArgs e) {
            PublishAllAsync().SilenceException<Exception>().DoNotWait();
        }

        #endregion
        private async Task InitializeData() {
            await PublishAllAsync();
        }

        #region variable subscription model

        private readonly Dictionary<VariableSubscriptionToken, List<VariableSubscription>> _subscribers = new Dictionary<VariableSubscriptionToken, List<VariableSubscription>>();

        public VariableSubscription Subscribe(
            int frameIndex,
            string variableExpression,
            Action<DebugEvaluationResult> executeAction) {

            var token = new VariableSubscriptionToken(frameIndex, variableExpression);

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
            await TaskUtilities.SwitchToBackgroundThread();

            var debugSessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IDebugSessionProvider>();

            if (_debugSession == null) {
                _debugSession = await debugSessionProvider.GetDebugSessionAsync(RSession);
                if (_debugSession == null) {
                    Debug.Fail("");
                    return;
                }
            }

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

            Debug.Assert(_debugSession != null);

            var stackFrames = await _debugSession.GetStackFramesAsync();
            var stackFrame = stackFrames.FirstOrDefault(f => f.Index == token.FrameIndex);

            if (stackFrame != null) {
                const DebugEvaluationResultFields fields = DebugEvaluationResultFields.Classes
                    | DebugEvaluationResultFields.Expression
                    | DebugEvaluationResultFields.TypeName
                    | (DebugEvaluationResultFields.Repr | DebugEvaluationResultFields.ReprStr)
                    | DebugEvaluationResultFields.Dim
                    | DebugEvaluationResultFields.Length;

                DebugEvaluationResult evaluation = await stackFrame.EvaluateAsync(token.Expression, fields: fields);

                foreach (var sub in subscriptions) {
                    try {
                        var action = sub.GetExecuteAction();
                        if (action != null) {
                            action(evaluation);
                        }
                    } catch (Exception e) {
                        Debug.Fail(e.ToString());
                        // swallow exception and continue
                    }
                }
            }
        }

        #endregion
    }
}
