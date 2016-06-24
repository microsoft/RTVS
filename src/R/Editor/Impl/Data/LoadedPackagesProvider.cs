// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

        [ImportingConstructor]
        public LoadedPackagesProvider(IRSessionProvider sessionProvider) : base(sessionProvider) { }

        protected override void SessionMutated() {
            UpdateListOfLoadedPackagesAsync().DoNotWait();
        }

        private async Task UpdateListOfLoadedPackagesAsync() {
            using (var e = await Session.BeginEvaluationAsync()) {
                string result;
                try {
                    result = await e.EvaluateAsync<string>("paste0(.packages(), collapse = ' ')", REvaluationKind.Normal);
                } catch (RException) {
                    return;
                }
                ParseSearchResponse(result);
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
