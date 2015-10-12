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
    public class VariableEvaluationContext
    {
        public const string GlobalEnv = ".GlobalEnv";

        public string Environment { get; set; }

        public string VariableName { get; set; }

        public string GetQueryString()
        {
            return string.Format(".rtvs.datainspect.eval(\"{0}\", env={1})\n", VariableName, this.Environment);
        }
    }

    public class VariableProvider
    {
        private bool _rSessionInitialized = false;
        private IRSession _rSession;
        private Variable _monitor;

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
                _rSessionInitialized = false;
            }

            // register event handler to new session
            _rSession = session;
            if (_rSession != null)
            {
                _rSession.BeforeRequest += RSession_BeforeRequest;
                _rSessionInitialized = false;

                Task t = RefreshVariableCollection();   // TODO: have a no-await wrapper to handle side effects
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

                using (var interactor = await _rSession.BeginEvaluationAsync())
                {
                    await interactor.EvaluateAsync(script, false);
                }

                _rSessionInitialized = true;
            }
        }

        private async Task RefreshVariableCollection()
        {
            if (_monitor == null) return;

            await EnsureRSessionInitialized();

            REvaluationResult response;
            using (var interactor = await _rSession.BeginEvaluationAsync())
            {
                response = await interactor.EvaluateAsync(
                    _monitor.EvaluationContext.GetQueryString(),
                    false);
            }

            if (response.ParseStatus == RParseStatus.OK)
            {
                var evaluation = Deserialize<REvaluation>(response.Result);
                if (evaluation != null)
                {
                    _monitor.Update(Variable.Create(evaluation, _monitor.EvaluationContext));
                }
            }
        }

        public async Task<Variable> EvaluateVariable(VariableEvaluationContext context) // TODO: rename to monitor
        {
            await EnsureRSessionInitialized();

            REvaluationResult response;
            using (var interactor = await _rSession.BeginEvaluationAsync())
            {
                response = await interactor.EvaluateAsync(context.GetQueryString(), false);
            }

            if (response.ParseStatus == RParseStatus.OK)
            {
                var evaluation = Deserialize<REvaluation>(response.Result);
                if (evaluation != null)
                {
                    var created = Variable.Create(evaluation, context);
                    _monitor = created;
                    return created;
                }
            }

            return null;    // TODO: error handling, throw?
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

        private static void DispatchInvoke(Action toInvoke, DispatcherPriority priority)
        {
            Action guardedAction =
                () =>
                {
                    try
                    {
                        toInvoke();
                    }
                    catch (Exception e)
                    {
                        Debug.Assert(false, "Guarded invoke caught exception", e.Message);
                    }
                };

            Application.Current.Dispatcher.BeginInvoke(guardedAction, priority);    // TODO: acquiring Application.Current.Dispatcher, create utility class for UI thread and use it
        }
    }
}
