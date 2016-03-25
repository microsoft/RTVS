// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.PackageManager.Model;
using Microsoft.R.Components.PackageManager.ViewModel;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Components.PackageManager.Implementation {
    internal class RPackageManager : IRPackageManager {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly Action _dispose;

        public IRPackageManagerVisualComponent VisualComponent { get; private set; }
        public IRPackageManagerViewModel _viewModel;

        public RPackageManager(IRInteractiveWorkflow interactiveWorkflow, Action dispose) {
            _interactiveWorkflow = interactiveWorkflow;
            _dispose = dispose;
        }

        public IRPackageManagerVisualComponent GetOrCreateVisualComponent(IRPackageManagerVisualComponentContainerFactory visualComponentContainerFactory, int instanceId = 0) {
            if (VisualComponent != null) {
                return VisualComponent;
            }

            VisualComponent = visualComponentContainerFactory.GetOrCreate(this, instanceId).Component;
            return VisualComponent;
        }

        public async Task<IEnumerable<RPackage>> GetInstalledPackagesAsync() {
            if (_interactiveWorkflow.RSession == null || !_interactiveWorkflow.RSession.IsHostRunning) {
                return Enumerable.Empty<RPackage>();
            }

            REvaluationResult result;
            using (IRSessionEvaluation eval = await _interactiveWorkflow.RSession.BeginEvaluationAsync()) {
                result = await eval.InstalledPackages();
            }

            return ParsePackages(result);
        }

        public async Task<IEnumerable<RPackage>> GetAvailablePackagesAsync() {
            if (_interactiveWorkflow.RSession == null || !_interactiveWorkflow.RSession.IsHostRunning) {
                return Enumerable.Empty<RPackage>();
            }

            REvaluationResult result;
            using (IRSessionEvaluation eval = await _interactiveWorkflow.RSession.BeginEvaluationAsync()) {
                result = await eval.AvailablePackages();
            }

            return ParsePackages(result);
        }

        private static IEnumerable<RPackage> ParsePackages(REvaluationResult result) {
            foreach (var jsonProp in ((JObject)result.JsonResult).Properties()) {
                var jsonObj = (JObject)jsonProp.Value;
                var pkg = jsonObj.ToObject<RPackage>();
                yield return pkg;
            }
        }

        public void Dispose() {
            _dispose();
        }
    }
}
