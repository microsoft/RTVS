#define USERINTERACTION // BUGBUG: BeginEvaluationAsync doesn't work!

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Repl.Session;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.VariableWindow;

namespace Microsoft.VisualStudio.R.Package.VariableExplorer
{
    class RVariableSession : IVariableSession
    {
        private IRSession _rSession;
        private IImmutableVariableCollection _variables;

        public RVariableSession(IRSession rSession)
        {
            _rSession = rSession;
            _variables = EmptyImmutableVariableCollection.Instance;

            this.Priority = 0;
            this.SessionDisplayName = "Global Envrironment";    // TODO: for now, global environment

            rSession.BeforeRequest += RSession_BeforeRequest;   // TODO: when to remove the event handler? watch out memory leak
            rSession.Disconnected += RSession_Disconnected;
        }

        #region RSession event handler

        private async void RSession_BeforeRequest(object sender, RBeforeRequestEventArgs e)
        {
            // await cause thie event handler returns and let event raiser run through
            // but it wait internally for varaible evaluation request to be processed
            await RefreshVariableCollection();
            if (VariablesChanged != null)
            {
                VariablesChanged(this, EventArgs.Empty);
            }
        }

        private void RSession_Disconnected(object sender, EventArgs e)
        {
            if (SessionClosed != null)
            {
                SessionClosed(this, EventArgs.Empty);
            }

            _rSession.BeforeRequest -= RSession_BeforeRequest;
            _rSession.Disconnected -= RSession_Disconnected;
            _rSession = null;   // TODO: add validation logic and prevent null reference exception
        }

        #endregion

        #region IVariableSession Support

        public int Priority
        {
            get;
            private set;
        }

        public string SessionDisplayName
        {
            get;
            private set;
        }

        public event EventHandler SessionClosed;
        public event EventHandler VariablesChanged;

        public Task<IExpression> GetExpressionAsync(string expression, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IImmutableVariableCollection> GetVariablesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_variables);
        }

        #endregion

#if USERINTERACTION
        private bool fRefreshing = false;

        private async Task RefreshVariableCollection()
        {

            if (fRefreshing)
            {
                return;
            }
            fRefreshing = true;
            using (var interactor = await _rSession.BeginInteractionAsync(false))
            {
                var response = await interactor.RespondAsync("ls.str(.GlobalEnv)\r\n");  // TODO: for now, global environment
                _variables = RVariableCollection.Parse(response);  // TODO: BUGBUG: make thread safe!

            }

            // no await
            Task.Run(async () =>
            {
                Task.Delay(100);
                fRefreshing = false;
            });
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
    }
    
}
