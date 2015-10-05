using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
    //[Export(typeof(IVariableSessionProvider))]
    class RVariableSessionProvider : IVariableSessionProvider
    {
        private List<RVariableSession> _variableSessions;
        private IRSession _rSession;

        public RVariableSessionProvider()
        {
            _variableSessions = new List<RVariableSession>();

            var sessionProvider = AppShell.Current.ExportProvider.GetExport<IRSessionProvider>().Value;
            sessionProvider.CurrentSessionChanged += RSessionProvider_CurrentChanged;
            _rSession = sessionProvider.Current;
            if (_rSession != null)
            {
                _variableSessions.Add(new RVariableSession(_rSession));
            }
        }

        #region IVariableSessionProvider Support

        public event EventHandler SessionsChanged;

        public IEnumerable<IVariableSession> GetSessions()
        {
            return _variableSessions;
        }

        #endregion

        private void RSessionProvider_CurrentChanged(object sender, EventArgs e)
        {
            var sessionProvider = sender as IRSessionProvider;
            Debug.Assert(sessionProvider != null);

            if (sessionProvider != null)
            {
                var session = sessionProvider.Current;
                if (!object.Equals(session, _rSession))
                {
                    _rSession = session;

                    // for now, supports only single session
                    _variableSessions.Clear();
                    _variableSessions.Add(new RVariableSession(_rSession));
                    if (SessionsChanged != null)
                    {
                        SessionsChanged(this, EventArgs.Empty);
                    }
                }
            }
        }
    }
}
