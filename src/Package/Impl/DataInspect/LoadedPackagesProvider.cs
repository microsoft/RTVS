using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    [Export(typeof(ILoadedPackagesProvider))]
    internal sealed class LoadedPackagesProvider : ILoadedPackagesProvider {
        private IRSessionProvider _sessionProvider;
        private IEnumerable<string> _loadedPackages = Enumerable.Empty<string>();

        public LoadedPackagesProvider() {
            _sessionProvider = VsAppShell.Current.ExportProvider.GetExport<IRSessionProvider>().Value;
            _sessionProvider.CurrentChanged += OnCurrentSessionChanged;
            ConnectToSession();
        }


        private void OnCurrentSessionChanged(object sender, EventArgs e) {
            ConnectToSession();
        }

        private void ConnectToSession() {
            if (_sessionProvider.Current != null) {
                _sessionProvider.Current.Mutated += OnSessionMutated;
                _sessionProvider.Current.Disposed += OnSessionDisposed;
            }
        }

        private void OnSessionDisposed(object sender, EventArgs e) {
            var session = sender as IRSession;
            if (session != null) {
                session.Mutated -= OnSessionMutated;
                session.Disposed -= OnSessionDisposed;
            }
        }

        private void OnSessionMutated(object sender, EventArgs e) {
            Task.Run(async () => await UpdateListOfLoadedPackagesAsync());
        }

        private async Task UpdateListOfLoadedPackagesAsync() {
            if (_sessionProvider.Current != null) {
                using (var e = await _sessionProvider.Current.BeginEvaluationAsync(isMutating: false)) {
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
