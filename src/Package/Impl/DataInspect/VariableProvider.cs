#define USERINTERACTION

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
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect
{
    public class VariableProvideContext
    {
        public string Environment { get; set; }
    }

    public class VariableProvider
    {
        private bool _rSessionInitialized = false;
        private IRSession _rSession;
        private List<Variable> _variables;

        private VariableView _view; // TODO: cut this dependency later

        public VariableProvider(VariableView view)
        {
            _view = view;
            _variables = new List<Variable>();

            var sessionProvider = AppShell.Current.ExportProvider.GetExport<IRSessionProvider>().Value;
            sessionProvider.CurrentSessionChanged += RSessionProvider_CurrentChanged;

            SetRSession(sessionProvider.Current);
        }

        #region Public

        public IList<Variable> Get(VariableProvideContext context)
        {
            if (_rSession == null)
            {
                return new List<Variable>();    // empty
            }
            else
            {
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
        private async void RSession_BeforeRequest(object sender, RBeforeRequestEventArgs e)
        {
            // await cause thie event handler returns and let event raiser run through
            // but it wait internally for varaible evaluation request to be processed
            await RefreshVariableCollection();
            _view.RefreshData();
        }

        /// <summary>
        /// IRSession.Disconnected handler. Used to unregister event handler from IRSession
        /// </summary>
        private void RSession_Disconnected(object sender, EventArgs e)
        {
            SetRSession(null);  // reset RSession reference
        }

        /// <summary>
        /// IRSessionProvider.CurrentSessionChanged handler. When current session changes, this is called
        /// </summary>
        private void RSessionProvider_CurrentChanged(object sender, EventArgs e)
        {
            var sessionProvider = sender as IRSessionProvider;
            Debug.Assert(sessionProvider != null);

            if (sessionProvider != null)
            {
                var session = sessionProvider.Current;
                if (!object.Equals(session, _rSession))
                {
                    SetRSession(session);
                }
            }
        }

        #endregion

        private void SetRSession(IRSession session)
        {
            // unregister event handler from old session
            if (_rSession != null)
            {
                _rSession.BeforeRequest -= RSession_BeforeRequest;
                _rSession.Disconnected -= RSession_Disconnected;
                _rSessionInitialized = false;
            }

            // register event handler to new session
            _rSession = session;
            if (_rSession != null)
            {
                _rSession.BeforeRequest += RSession_BeforeRequest;
                _rSession.Disconnected += RSession_Disconnected;
                _rSessionInitialized = false;
            }

            if (SessionsChanged != null)
            {
                SessionsChanged(this, EventArgs.Empty);
            }
        }

        private async Task EnsureRSessionInitialized()
        {
            if (!_rSessionInitialized)
            {
                using (var interactor = await _rSession.BeginInteractionAsync(false))
                {
                    string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string filePath = Path.Combine(dir, @"DataInspect\DataInspect.R").Replace('\\', '/');
                    string script = string.Format("source('{0}')\r\n", filePath);

                    await interactor.RespondAsync(script);
                }

                _rSessionInitialized = true;
            }
        }

#if USERINTERACTION
        private bool fRefreshing = false;

        private async Task RefreshVariableCollection()
        {
            if (fRefreshing)
            {
                return;
            }
            fRefreshing = true;

            var variables = new List<Variable>();

            try
            {
                await EnsureRSessionInitialized();

                using (var interactor = await _rSession.BeginInteractionAsync(false))
                {
                    var response = await interactor.RespondAsync(".rtvs.datainspect.env_vars(.GlobalEnv)\r\n");  // TODO: for now, global environment
                    var evaluations = Deserialize<List<REvaluation>>(response);
                    if (evaluations != null)
                    {
                        foreach (var evaluation in evaluations)
                        {
                            var instance = Variable.Create(evaluation);
                            variables.Add(instance);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to get variables:{0}", e.Message);
            }

            _variables = variables; // if fails, empty variables window

            #pragma warning disable 4014
            Task.Run(async () =>            // no await
            {
                await Task.Delay(100);
                fRefreshing = false;        // TODO: BUGBUG: dirty workaround. yikes!
            });
            #pragma warning restore 4014
        }

#else
        private async Task RefreshVariableCollection()
        {
            await EnsureRSessionInitialized();

            using (var interactor = await _rSession.BeginEvaluationAsync())
            {
                var response = await interactor.EvaluateAsync(".rtvs.datainspect.env_vars(.GlobalEnv)\r\n", false);
                if (response.ParseStatus == RParseStatus.OK)
                {
                    var evaluations = Deserialize(response.Result);
                    if (evaluations != null)
                    {
                        var variables = new List<Variable>();
                        foreach (var evaluation in evaluations)
                        {
                            var instance = Variable.Create(evaluation);
                            variables.Add(instance);
                        }
                        _variables = variables;
                    }
                }
            }
        }
#endif

        private static T Deserialize<T>(string response) where T : class
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(response)))
                {
                    return (T)serializer.ReadObject(stream);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());

                return null;
            }
        }
    }
}
