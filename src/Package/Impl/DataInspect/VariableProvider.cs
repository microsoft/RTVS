using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Repl.Session;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class VariableProvideContext {
        public string Environment { get; set; }
    }

    public class VariableProvider {
        private bool _rSessionInitialized = false;
        private IRSession _rSession;
        private List<Variable> _variables;

        private VariableView _view; // TODO: cut this dependency later

        public VariableProvider(VariableView view) {
            _view = view;
            _variables = new List<Variable>();

            var sessionProvider = AppShell.Current.ExportProvider.GetExport<IRSessionProvider>().Value;
            sessionProvider.CurrentSessionChanged += RSessionProvider_CurrentChanged;

            SetRSession(sessionProvider.Current);
        }

        #region Public

        public IList<Variable> Get(VariableProvideContext context) {
            if (_rSession == null) {
                return new List<Variable>();    // empty
            } else {
                return _variables;
            }
        }

        /// <summary>
        /// R current session change triggers this SessionsChanged event
        /// </summary>
        public event EventHandler SessionsChanged;

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
        /// IRSession.Disconnected handler. Used to unregister event handler from IRSession
        /// </summary>
        private void RSession_Disconnected(object sender, EventArgs e) {
            SetRSession(null);  // reset RSession reference
        }

        /// <summary>
        /// IRSessionProvider.CurrentSessionChanged handler. When current session changes, this is called
        /// </summary>
        private void RSessionProvider_CurrentChanged(object sender, EventArgs e) {
            var sessionProvider = sender as IRSessionProvider;
            Debug.Assert(sessionProvider != null);

            if (sessionProvider != null) {
                var session = sessionProvider.Current;
                if (!object.Equals(session, _rSession)) {
                    SetRSession(session);
                }
            }
        }

        #endregion

        private void SetRSession(IRSession session) {
            // unregister event handler from old session
            if (_rSession != null) {
                _rSession.BeforeRequest -= RSession_BeforeRequest;
                _rSession.Disconnected -= RSession_Disconnected;
                _rSessionInitialized = false;
            }

            // register event handler to new session
            _rSession = session;
            if (_rSession != null) {
                _rSession.BeforeRequest += RSession_BeforeRequest;
                _rSession.Disconnected += RSession_Disconnected;
                _rSessionInitialized = false;

                Task t = RefreshVariableCollection();   // TODO: have a no-await wrapper to handle side effects
            }

            if (SessionsChanged != null) {
                SessionsChanged(this, EventArgs.Empty);
            }
        }

        private async Task EnsureRSessionInitialized() {
            if (!_rSessionInitialized) {

                string script = null;

                var assembly = this.GetType().Assembly;
                using (var stream = assembly.GetManifestResourceStream("Microsoft.VisualStudio.R.Package.DataInspect.DataInspect.R"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        script = reader.ReadToEnd();
                    }
                }
                script += "\n";

                using (var interactor = await _rSession.BeginEvaluationAsync()) {
                    await interactor.EvaluateAsync(script, false);
                }

                _rSessionInitialized = true;
            }
        }

        private async Task RefreshVariableCollection() {
            await EnsureRSessionInitialized();

            REvaluationResult response;
            using (var interactor = await _rSession.BeginEvaluationAsync()) {
                response = await interactor.GetGlobalEnvironmentVariables();
            }

            if (response.ParseStatus == RParseStatus.OK) {
                var evaluations = Deserialize<List<REvaluation>>(response.Result);
                if (evaluations != null) {
                    _variables = evaluations.Select(Variable.Create).ToList();
                }
            }

            _view.RefreshData();
        }

        private static T Deserialize<T>(string response) where T : class {
            try {
                var serializer = new DataContractJsonSerializer(typeof(T));
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(response))) {
                    return (T)serializer.ReadObject(stream);
                }
            } catch (Exception e) {
                Debug.WriteLine(e.ToString());

                return null;
            }
        }
    }
}
