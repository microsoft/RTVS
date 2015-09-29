using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.VariableWindow;

namespace Microsoft.VisualStudio.R.Package.VariableExplorer
{
    class RVariableSessionProvider : IVariableSessionProvider
    {
        private List<RVariableSession> _variableSessions;

        #region IVariableSessionProvider Support

        public event EventHandler SessionsChanged;

        public IEnumerable<IVariableSession> GetSessions()
        {
            EnsureRSession();

            if (_variableSessions == null)
            {
                // Supports single session for now
                _variableSessions = new List<RVariableSession>() { new RVariableSession(_session) };
            }

            return _variableSessions;
        }

        #endregion

        private IRSessionProvider _sessionProvider;
        private IRSession _session;
        private IRSession EnsureRSession()
        {
            if (_session == null)
            {
                if (_sessionProvider == null)
                {
                    _sessionProvider = AppShell.Current.ExportProvider.GetExport<IRSessionProvider>().Value;
                }

                _session = _sessionProvider.Current;
                if (_session == null)
                {
                    _session = _sessionProvider.Create(0); // TODO: fow now, only single R session supported. Hard-coded 0
                }
            }

            Debug.Assert(_session != null);
            return _session;
        }


        public static async void foo()  // temporary method to call in VS IDE
        {
            var variableSessionProvider = new RVariableSessionProvider();
            var sessions = variableSessionProvider.GetSessions();

            foreach (var session in sessions)
            {
                var variableCollection = await session.GetVariablesAsync(CancellationToken.None);  // TODO: no cancellation for now
                int count = variableCollection.Count;

                for (int i = 0; i < count; i++)
                {
                    var variable = await variableCollection.GetAsync(i, CancellationToken.None);
                    Debug.WriteLine("Variable: {0} {1}", variable.Expression, variable.TypeName);
                }
            }
        }
    }
}
