using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.R.Debugger;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Shell;
using Newtonsoft.Json;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class VariableChangedArgs : EventArgs {
        public EvaluationWrapper NewVariable { get; set; }
        }

    internal class VariableProvider : IDisposable {
        #region members and ctor

        private IRSession _rSession;
        private DebugSession _debugSession;

        public VariableProvider() {
            var sessionProvider = AppShell.Current.ExportProvider.GetExport<IRSessionProvider>().Value;
            sessionProvider.CurrentSessionChanged += RSessionProvider_CurrentChanged;

            IdleTimeAction.Create(async () => {
                await SetRSession(sessionProvider.Current);
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

        #endregion

        #region RSession related event handler

        /// <summary>
        /// IRSession.BeforeRequest handler. At each interaction request, this is called.
        /// Used to queue another request to refresh variables after the interaction request.
        /// </summary>
        private async void RSession_BeforeRequest(object sender, RRequestEventArgs e) {
            await RefreshVariableCollection();
        }

        /// <summary>
        /// IRSessionProvider.CurrentSessionChanged handler. When current session changes, this is called
        /// </summary>
        private async void RSessionProvider_CurrentChanged(object sender, EventArgs e) {
            var sessionProvider = sender as IRSessionProvider;
            Debug.Assert(sessionProvider != null);

            if (sessionProvider != null) {
                var session = sessionProvider.Current;
                if (!object.Equals(session, _rSession)) {
                    await SetRSession(session);
                }
            }
        }

        #endregion

        private async Task InitializeData() {
            var debugSessionProvider = AppShell.Current.ExportProvider.GetExport<IDebugSessionProvider>().Value;

            if (_debugSession != null) {
                _debugSession.Dispose();
                _debugSession = null;
            }

            _debugSession = await debugSessionProvider.GetDebugSessionAsync(_rSession);

            await RefreshVariableCollection();
        }

        private async Task SetRSession(IRSession session) {
            // cleans up old RSession
            if (_rSession != null) {
                _rSession.BeforeRequest -= RSession_BeforeRequest;
            }

            // set new RSession
            _rSession = session;
            if (_rSession != null) {
                _rSession.BeforeRequest += RSession_BeforeRequest;
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

                LastEvaluation = new EvaluationWrapper(evaluation);

                if (VariableChanged != null) {
                        VariableChanged(
                            this,
                        new VariableChangedArgs() { NewVariable = LastEvaluation });
        }
            }
            }

        public void Dispose() {
            if (_debugSession != null) {
                _debugSession.Dispose();
            }
            _rSession = null;
        }
    }
}
