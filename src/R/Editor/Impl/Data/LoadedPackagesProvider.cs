using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Editor.Data {
    [Export(typeof(ILoadedPackagesProvider))]
    internal sealed class LoadedPackagesProvider : RSessionChangeWatcher, ILoadedPackagesProvider {
        private IEnumerable<string> _loadedPackages = Enumerable.Empty<string>();

        protected override void SessionMutated() {
            Task.Run(async () => await UpdateListOfLoadedPackagesAsync());
        }

        private async Task UpdateListOfLoadedPackagesAsync() {
            if (Session != null && Session.IsHostRunning) {
                using (var e = await Session.BeginEvaluationAsync(isMutating: false)) {
                    REvaluationResult result = await e.EvaluateAsync("paste0(.packages(), collapse = ' ')");
                    if (result.ParseStatus == RParseStatus.OK && result.Error == null && result.StringResult != null) {
                        ParseSearchResponse(result.StringResult);
                    }
                }
            }
        }

        private void ParseSearchResponse(string response) {
            EditorShell.Current.DispatchOnUIThread(() => {
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
