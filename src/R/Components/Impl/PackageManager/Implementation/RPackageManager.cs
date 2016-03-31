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

        public async Task<IReadOnlyList<RPackage>> GetInstalledPackagesAsync() {
            return await GetPackages(async (eval) => await eval.InstalledPackages());
        }

        public async Task<IReadOnlyList<RPackage>> GetAvailablePackagesAsync() {
            return await GetPackages(async (eval) => await eval.AvailablePackages());
        }

        public async Task GetAdditionalPackageInfoAsync(RPackage pkg) {
            var uri = GetPackageWebIndexUri(pkg);
            await RPackageWebParser.RetrievePackageInfo(uri, pkg);
        }

        public void InstallPackage(string name, string libraryPath) {
            string script;
            if (string.IsNullOrEmpty(libraryPath)) {
                script = string.Format("install.packages({0})", name.ToRStringLiteral());
            } else {
                script = string.Format("install.packages({0}, lib={1})", name.ToRStringLiteral(), libraryPath.ToRPath().ToRStringLiteral());
            }

            _interactiveWorkflow.Operations.EnqueueExpression(script, true);
        }

        public void UninstallPackage(string name, string libraryPath) {
            string script;
            if (string.IsNullOrEmpty(libraryPath)) {
                script = string.Format("remove.packages({0})", name.ToRStringLiteral());
            } else {
                script = string.Format("remove.packages({0}, lib={1})", name.ToRStringLiteral(), libraryPath.ToRPath().ToRStringLiteral());
            }

            _interactiveWorkflow.Operations.EnqueueExpression(script, true);
        }

        public void LoadPackage(string name, string libraryPath) {
            string script;
            if (string.IsNullOrEmpty(libraryPath)) {
                script = string.Format("library({0})", name.ToRStringLiteral());
            } else {
                script = string.Format("library({0}, lib.loc={1})", name.ToRStringLiteral(), libraryPath.ToRPath().ToRStringLiteral());
            }

            _interactiveWorkflow.Operations.EnqueueExpression(script, true);
        }

        public void UnloadPackage(string name) {
            string script = string.Format("unloadNamespace({0})", name.ToRStringLiteral());

            _interactiveWorkflow.Operations.EnqueueExpression(script, true);
        }

        public async Task<string[]> GetLoadedPackagesAsync() {
            if (!_interactiveWorkflow.RSession.IsHostRunning) {
                return new string[0];
            }

            try {
                using (var eval = await _interactiveWorkflow.RSession.BeginEvaluationAsync()) {
                    var result = await eval.LoadedPackages();
                    if (result.ParseStatus != RParseStatus.OK || result.Error != null) {
                        throw new InvalidOperationException(result.ToString());
                    }

                    return ((JArray)result.JsonResult)
                        .Select(p => (string)((JValue)p).Value)
                        .ToArray();
                }
            } catch (OperationCanceledException) {
                return new string[0];
            }
        }

        public async Task<string[]> GetLibraryPathsAsync() {
            if (!_interactiveWorkflow.RSession.IsHostRunning) {
                return new string[0];
            }

            try {
                using (var eval = await _interactiveWorkflow.RSession.BeginEvaluationAsync()) {
                    var result = await eval.LibraryPaths();
                    if (result.ParseStatus != RParseStatus.OK || result.Error != null) {
                        throw new InvalidOperationException(result.ToString());
                    }

                    return ((JArray)result.JsonResult)
                        .Select(p => (string)((JValue)p).Value)
                        .ToArray();
                }
            } catch (OperationCanceledException) {
                return new string[0];
            }
        }

        private static Uri GetPackageWebIndexUri(RPackage pkg) {
            // For example, if 'Repository' is:
            // "https://cloud.r-project.org/src/contrib"
            // Then the URI to the index page is:
            // "https://cloud.r-project.org/web/packages/<packagename>/index.html"
            var contribUrl = pkg.Repository;
            if (!contribUrl.EndsWith("/")) {
                contribUrl += "/";
            }

            return new Uri(new Uri(contribUrl), string.Format("../../web/packages/{0}/index.html", pkg.Package));
        }

        private async Task<IReadOnlyList<RPackage>> GetPackages(Func<IRSessionEvaluation, Task<REvaluationResult>> fetchFunc) {
            if (!_interactiveWorkflow.RSession.IsHostRunning) {
                return new List<RPackage>().AsReadOnly();
            }

            try {
                using (var eval = await _interactiveWorkflow.RSession.BeginEvaluationAsync()) {
                    var result = await fetchFunc(eval);
                    if (result.ParseStatus != RParseStatus.OK || result.Error != null) {
                        throw new InvalidOperationException(result.ToString());
                    }
                    return ((JObject)result.JsonResult).Properties()
                        .Select(p => p.Value.ToObject<RPackage>())
                        .ToList().AsReadOnly();
                }
            } catch (OperationCanceledException) {
                return new List<RPackage>().AsReadOnly();
            }
        }

        public void Dispose() {
            _dispose();
        }
    }
}
