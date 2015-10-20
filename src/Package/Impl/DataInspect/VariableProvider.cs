using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Shell;
using Newtonsoft.Json;

namespace Microsoft.VisualStudio.R.Package.DataInspect
{
    internal class VariableEvaluationContext
    {
        public const string GlobalEnv = ".GlobalEnv";

        public string Environment { get; set; }

        public string VariableName { get; set; }

        public string GetQueryString()
        {
            return string.Format(".rtvs.datainspect.eval(\"{0}\", env={1})\n", VariableName, this.Environment);
        }
    }

    internal class VariableChangedArgs : EventArgs
    {
        public REvaluation NewVariable { get; set; }
    }

    internal class VariableProvider
    {
        public static VariableProvider Current => _instance.Value;
        public VariableEvaluationContext GlobalContext { get; private set; }

        private static Lazy<VariableProvider> _instance = new Lazy<VariableProvider>(() => new VariableProvider());
        private IRSession _rSession;
        private VariableEvaluationContext _monitorContext;

        public VariableProvider()
        {
            var sessionProvider = AppShell.Current.ExportProvider.GetExport<IRSessionProvider>().Value;
            sessionProvider.CurrentSessionChanged += RSessionProvider_CurrentChanged;

            IdleTimeAction.Create(async () =>
            {
                await SetRSession(sessionProvider.Current);   // TODO: find a place to SetRSession to null, watch out memory leak
                await InitializeData();
            }, 10, typeof(VariableProvider));
        }

        #region Public
        /// <summary>
        /// R current session change triggers this SessionsChanged event
        /// </summary>
        public event EventHandler SessionsChanged;
        public event EventHandler<VariableChangedArgs> VariableChanged;
        #endregion

        #region RSession related event handler

        /// <summary>
        /// IRSession.BeforeRequest handler. At each interaction request, this is called.
        /// Used to queue another request to refresh variables after the interaction request.
        /// </summary>
        private async void RSession_BeforeRequest(object sender, RRequestEventArgs e)
        {
            await RefreshVariableCollection();
        }

        /// <summary>
        /// IRSessionProvider.CurrentSessionChanged handler. When current session changes, this is called
        /// </summary>
        private async void RSessionProvider_CurrentChanged(object sender, EventArgs e)
        {
            var sessionProvider = sender as IRSessionProvider;
            Debug.Assert(sessionProvider != null);

            if (sessionProvider != null)
            {
                var session = sessionProvider.Current;
                if (!object.Equals(session, _rSession))
                {
                    await SetRSession(session);
                }
            }
        }

        #endregion

        private async Task InitializeData()
        {
            GlobalContext = new VariableEvaluationContext()
            {
                Environment = VariableEvaluationContext.GlobalEnv,
                VariableName = VariableEvaluationContext.GlobalEnv
            };

            await SetMonitorContext(GlobalContext);
        }

        private async Task SetRSession(IRSession session)
        {
            // unregister event handler from old session
            if (_rSession != null)
            {
                _rSession.BeforeRequest -= RSession_BeforeRequest;
            }

            // register event handler to new session
            _rSession = session;
            if (_rSession != null)
            {
                _rSession.BeforeRequest += RSession_BeforeRequest;
                Task t = RefreshVariableCollection();   // TODO: have a no-await wrapper to handle side effects
            }

            await InitializeData();

            if (SessionsChanged != null)
            {
                SessionsChanged(this, EventArgs.Empty);
            }
        }

        private async Task RefreshVariableCollection()
        {
            if (_rSession == null)
            {
                return;
            }

            REvaluationResult response;
            using (var interactor = await _rSession.BeginEvaluationAsync())
            {
                response = await interactor.EvaluateAsync(
                    _monitorContext.GetQueryString(),
                    false);
            }

            if (response.ParseStatus == RParseStatus.OK)
            {
                var evaluation = Deserialize<REvaluation>(response.Result);
                if (evaluation != null)
                {
                    if (VariableChanged != null)
                    {
                        VariableChanged(
                            this,
                            new VariableChangedArgs() { NewVariable = evaluation });
                    }
                }
            }
        }

        public async Task SetMonitorContext(VariableEvaluationContext context) // TODO: rename to monitor
        {
            _monitorContext = context;
            await RefreshVariableCollection();
        }

        private static T Deserialize<T>(string response)
        {
            if (response == null)
            {
                return default(T);
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(response);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());

                return default(T);
            }
        }
    }
}
