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
        private readonly IRSession _rSession;

        public RVariableSession(IRSession rSession)
        {
            _rSession = rSession;

            this.Priority = 0;
            this.SessionDisplayName = "Global Envrironment";    // TODO: for now, global environment
        }

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

        public async Task<IImmutableVariableCollection> GetVariablesAsync(CancellationToken cancellationToken)
        {
            var interaction = await _rSession.BeginInteractionAsync(false);
            var response = await interaction.RespondAsync("ls.str(.GlobalEnv)\r\n");    // TODO: for now, global environment
            var variableCollection = RVariableCollection.Parse(response);
            return variableCollection;
        }

        #endregion
    }
}
