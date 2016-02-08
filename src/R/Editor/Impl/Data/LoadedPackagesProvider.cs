using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Common.Core;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Host.Client;
using System.Threading;

namespace Microsoft.R.Editor.Data {
    [Export(typeof(ILoadedPackagesProvider))]
    internal sealed class LoadedPackagesProvider : RSessionChangeWatcher, ILoadedPackagesProvider {
        private IEnumerable<string> _loadedPackages = Enumerable.Empty<string>();

        protected override void SessionMutated() {
            UpdateListOfLoadedPackagesAsync().DoNotWait();
        }

        private async Task UpdateListOfLoadedPackagesAsync() {
            using (var e = await Session.BeginEvaluationAsync(isMutating: false)) {
                REvaluationResult result = await e.EvaluateAsync("paste0(.packages(), collapse = ' ')");
                if (result.ParseStatus == RParseStatus.OK && result.Error == null && result.StringResult != null) {
                    ParseSearchResponse(result.StringResult);
                }
            }
        }

        private void ParseSearchResponse(string response) {
            var loadedPackages = response.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Interlocked.Exchange(ref _loadedPackages, loadedPackages);
        }

        #region ILoadedPackagesProvider
        public IEnumerable<string> GetPackageNames() {
            return _loadedPackages;
        }
        #endregion
    }
}
