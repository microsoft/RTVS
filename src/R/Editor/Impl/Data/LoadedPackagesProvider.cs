// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Completion;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.R.Editor.Data {
    [Export(typeof(ILoadedPackagesProvider))]
    internal sealed class LoadedPackagesProvider : RSessionChangeWatcher, ILoadedPackagesProvider {
        private IEnumerable<string> _loadedPackages = Enumerable.Empty<string>();

        [ImportingConstructor]
        public LoadedPackagesProvider(IRInteractiveWorkflowProvider workflowProvider) : base(workflowProvider) { }

        protected override void SessionMutated() {
            UpdateListOfLoadedPackagesAsync().DoNotWait();
        }

        private async Task UpdateListOfLoadedPackagesAsync() {
            string result;
            try {
                result = await Session.EvaluateAsync<string>("paste0(.packages(), collapse = ' ')", REvaluationKind.Normal);
            } catch (RHostDisconnectedException) {
                return;
            } catch (RException) {
                return;
            }
            ParseSearchResponse(result);
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
