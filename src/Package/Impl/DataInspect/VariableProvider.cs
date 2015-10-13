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
using System.Windows.Threading;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Repl.Session;
using Microsoft.VisualStudio.R.Package.Shell;

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
        private IRSession _rSession;
        private VariableEvaluationContext _monitorContext;

        public VariableProvider()
        {
            var sessionProvider = AppShell.Current.ExportProvider.GetExport<IRSessionProvider>().Value;
            sessionProvider.CurrentSessionChanged += RSessionProvider_CurrentChanged;

            SetRSession(sessionProvider.Current);   // TODO: f ind a place to SetRSession to null, watch out memory leak
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
            }

            // register event handler to new session
            _rSession = session;
            if (_rSession != null)
            {
                _rSession.BeforeRequest += RSession_BeforeRequest;

                Task t = RefreshVariableCollection();   // TODO: have a no-await wrapper to handle side effects
            }

            if (SessionsChanged != null)
            {
                SessionsChanged(this, EventArgs.Empty);
            }
        }

        private async Task RefreshVariableCollection()
        {
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
