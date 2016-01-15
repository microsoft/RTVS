using System;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Editor.Data {
    public abstract class RSessionChangeWatcher {
        protected IRSessionProvider SessionProvider { get; }
        protected IRSession Session { get; private set; }

        public RSessionChangeWatcher() {
            SessionProvider = EditorShell.Current.ExportProvider.GetExport<IRSessionProvider>().Value;
            Session = SessionProvider.GetOrCreate(GuidList.InteractiveWindowRSessionGuid, null);
            Session.Disposed += OnSessionDisposed; ;
            ConnectToSession();
        }

        private void ConnectToSession() {
            if (Session != null) {
                Session.Mutated += OnSessionMutated;
                Session.Disposed += OnSessionDisposed;
            }
        }

        private void OnSessionDisposed(object sender, EventArgs e) {
            var session = sender as IRSession;
            if (session != null) {
                session.Mutated -= OnSessionMutated;
                session.Disposed -= OnSessionDisposed;
            }
            Session = null;
        }

        private void OnSessionMutated(object sender, EventArgs e) {
            SessionMutated();
        }

        protected abstract void SessionMutated();
    }
}
