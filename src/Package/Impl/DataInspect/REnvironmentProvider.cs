// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Shell;
using Newtonsoft.Json.Linq;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class REnvironmentProvider : IREnvironmentProvider, IDisposable {
        private IRSession _rSession;

        public REnvironmentProvider(IRSession session) {
            _rSession = session;
            _rSession.Mutated += RSession_Mutated;

            GetEnvironmentAsync().DoNotWait();
        }

        private void RSession_Mutated(object sender, EventArgs e) {
            if (EnvironmentChanged != null) {
                GetEnvironmentAsync().DoNotWait();
            }
        }

        private async Task GetEnvironmentAsync() {
            await TaskUtilities.SwitchToBackgroundThread();

            REvaluationResult result;
            using (var evaluation = await _rSession.BeginEvaluationAsync()) {
                result = await evaluation.EvaluateAsync("rtvs:::getEnvironments(sys.frame(sys.nframe()))", REvaluationKind.Json);
            }

            Debug.Assert(result.JsonResult != null);

            var envs = new List<REnvironment>();
            var jarray = result.JsonResult as JArray;
            if (jarray != null) {
                foreach (var jsonItem in jarray) {
                    envs.Add(new REnvironment(jsonItem));
                }
            }

            VsAppShell.Current.DispatchOnUIThread(() => {
                OnEnvironmentChanged(new REnvironmentChangedEventArgs(envs));
            });
        }

        private void OnEnvironmentChanged(REnvironmentChangedEventArgs args) {
            if (EnvironmentChanged != null) {
                EnvironmentChanged(this, args);
            }
        }

        #region IREnvironmentProvider

        public event EventHandler<REnvironmentChangedEventArgs> EnvironmentChanged;

        #endregion

        public void Dispose() {
            _rSession.Mutated -= RSession_Mutated;
            _rSession = null;
        }
    }
}
