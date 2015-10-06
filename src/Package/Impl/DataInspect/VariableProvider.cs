#define USERINTERACTION

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Controls
{
    public class VariableProvideContext
    {
        public static VariableProvideContext GlobalEnvironment = new VariableProvideContext() { Environment = ".GlobalEnv" };

        public string Environment { get; set; }
    }

    public class VariableProvider
    {
        private bool _rSessionInitialized = false;
        private IRSession _rSession;
        private List<Variable> _variables;

        private VariableView _view; // TODO: cut this dependency later

        private static List<Variable> EmptyVariables = new List<Variable>();

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
                return EmptyVariables;
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

        private async void SetRSession(IRSession session)
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
                    await interactor.RespondAsync(InitializingRScript);
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

            await EnsureRSessionInitialized();

            using (var interactor = await _rSession.BeginInteractionAsync(false))
            {
                var response = await interactor.RespondAsync(".rtvs.datainspect.env_vars(.GlobalEnv)\r\n");  // TODO: for now, global environment
                var evaluations = Deserialize(response);
                if (evaluations == null)
                {
                    _variables = EmptyVariables;
                }
                else
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

            // no await
            Task.Run(async () =>
            {
                await Task.Delay(100);
                fRefreshing = false;        // TODO: BUGBUG: dirty workaround. yikes!
            });
        }

        private static List<REvaluation> Deserialize(string response)
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(List<REvaluation>));
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(response)))
                {
                    return (List<REvaluation>)serializer.ReadObject(stream);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: Deserialization error:{0}\r\n{1}", response, e.Message);

                return null;
            }
        }

#else
        private async Task RefreshVariableCollection()
        {
            using (var interactor = await _rSession.BeginEvaluationAsync())
            {
                var response = await interactor.EvaluateAsync("ls.str(.GlobalEnv)\r\n");
                if (response.ParseStatus == RParseStatus.OK)
                {
                    _variables = RVariableCollection.Parse(response.Result);  // TODO: BUGBUG: make thread safe!
                }
            }
        }
#endif

        private readonly string InitializingRScript =
@".rtvs.datainspect.eval_into <<- function(con, expr, env) {
  obj <- eval(parse(text = expr), env);
  con_repr <- textConnection(NULL, open = ""w"");
  tryCatch({
    dput(obj, con_repr);
    repr <-textConnectionValue(con_repr);
  }, finally = {
    close(con_repr);
  });

  cat('""name"": ""', file=con, sep='');cat(expr, file=con);cat('""', file=con, sep='');
  cat(',""class"": ""', file=con, sep='');cat(class(expr), file=con);cat('""', file=con, sep='');
  cat(',""value"": ', file=con, sep='');dput(paste(repr, collapse=''), file=con);
  cat(',""type"": ""', file=con, sep='');cat(typeof(obj), file=con);cat('""', file=con, sep='');
}
.rtvs.datainspect.eval <<- function(expr, env) {
  con <-textConnection(NULL, open = ""w"");
  json <-""{}"";
  tryCatch({
    cat('{', file=con, sep='');
    .rtvs.datainspect.eval_into(con, expr, env);
    cat('}\n', file=con, sep='');
    json <-textConnectionValue(con);
  }, finally = {
    close(con);
  });
  cat(json);
}
.rtvs.datainspect.env_vars <<-function(env) {
  con <-textConnection(NULL, open = ""w"");
  json <-""{}"";
  tryCatch({
    cat('[', file=con, sep='');
    is_first <-TRUE;
    for (varname in ls(env))
    {
      if (is_first) {
        is_first <-FALSE;
      } else {
        cat(', ', file=con, sep='');
      }
      cat('{', file=con, sep='');
      .rtvs.datainspect.eval_into(con, varname, env);
      cat('}', file=con, sep='');
    }
    cat(']\n', file=con, sep='');
    json <-textConnectionValue(con);
  }, finally = {
    close(con);
  });
  cat(json);
}
";
    }
}
