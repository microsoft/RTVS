using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    [Export(typeof(ILoadedPackagesProvider))]
    internal sealed class LoadedPackagesProvider : ILoadedPackagesProvider {
        private IRInteractiveProvider _interactiveProvider;
        private IRSession _session;
        private IEnumerable<string> _loadedPackages = Enumerable.Empty<string>();

        public LoadedPackagesProvider() {
            _interactiveProvider = VsAppShell.Current.ExportProvider.GetExport<IRInteractiveProvider>().Value;
            _session = _interactiveProvider.GetOrCreate().RSession;
            _session.Disposed += OnSessionDisposed; ;
            ConnectToSession();
        }

        private void ConnectToSession() {
            if (_session != null) {
                _session.Mutated += OnSessionMutated;
                _session.Disposed += OnSessionDisposed;
            }
        }

        private void OnSessionDisposed(object sender, EventArgs e) {
            var session = sender as IRSession;
            if (session != null) {
                session.Mutated -= OnSessionMutated;
                session.Disposed -= OnSessionDisposed;
            }
            _session = null;
        }

        private void OnSessionMutated(object sender, EventArgs e) {
            Task.Run(async () => await UpdateListOfLoadedPackagesAsync());
        }

        private async Task UpdateListOfLoadedPackagesAsync() {
            if (_session != null && _session.IsHostRunning) {
                using (var e = await _session.BeginEvaluationAsync(isMutating: false)) {
                    REvaluationResult result = await e.EvaluateAsync("paste0(.packages(), collapse = ' ')");
                    if (result.ParseStatus == RParseStatus.OK && result.Error == null && result.StringResult != null) {
                        ParseSearchResponse(result.StringResult);
                    }
                }
            }
        }

        private void ParseSearchResponse(string response) {
            VsAppShell.Current.DispatchOnUIThread(() => {
                _loadedPackages = response.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            });
        }

        #region ILoadedPackagesProvider
        public IEnumerable<string> GetPackageNames() {
            return _loadedPackages;
        }
        #endregion
    }
}
