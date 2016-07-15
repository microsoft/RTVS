// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Support.Help.Functions {
    /// <summary>
    /// Contains index of function to package improving performance of 
    /// locating package that contains the function documentation.
    /// </summary>
    [Export(typeof(IFunctionIndex))]
    public sealed partial class FunctionIndex {
        private static readonly Guid IndexingSessionId = new Guid("1BDD5A38-8A39-468C-B571-870F36E5E6C3");
        private readonly ICoreShell _shell;
        private readonly IRSessionProvider _sessionProvider;
        private readonly IPackageIndex _packageIndex;
        private readonly IIntellisenseRHost _host;
        private readonly BinaryAsyncLock _buildIndexLock = new BinaryAsyncLock();

        public static readonly IEnumerable<string> PreloadedPackages = new string[]
            { "base", "stats", "utils", "graphics", "datasets", "methods" };

        [ImportingConstructor]
        public FunctionIndex(ICoreShell shell, IRSessionProvider sessionProvider, IPackageIndex packageIndex, IFunctionRdDataProvider rdDataProfider, IIntellisenseRHost host) {
            _shell = shell;
            _sessionProvider = sessionProvider;
            _packageIndex = packageIndex;
            _functionRdDataProvider = rdDataProfider;
            _host = host;
        }

        public bool IsReady { get; private set; }

        public async Task BuildIndexAsync() {
            IsReady = await _buildIndexLock.WaitAsync();
            if (!IsReady) {
                var startTotalTime = DateTime.Now;

                await _host.CreateSessionAsync();
                Debug.WriteLine("R function index host start: {0} ms", (DateTime.Now - startTotalTime).TotalMilliseconds);

                // First populate index for popular packages that are commonly preloaded
                var startTime = DateTime.Now;
                await AddFunctionsFromPackages(FunctionIndex.PreloadedPackages);
                Debug.WriteLine("R functions popular: {0} ms", (DateTime.Now - startTime).TotalMilliseconds);

                // Now handle remaining packages
                await _packageIndex.BuildIndexAsync(this, _host.Session);

                startTime = DateTime.Now;
                var packageNames = _packageIndex.Packages.Where(p => !PreloadedPackages.Contains(p.Name)).Select(p => p.Name);
                await AddFunctionsFromPackages(packageNames);
                Debug.WriteLine("R functions remaining: {0} ms", (DateTime.Now - startTime).TotalMilliseconds);

                Debug.WriteLine("R functions index total: {0} ms", (DateTime.Now - startTotalTime).TotalMilliseconds);
                IsReady = true;
            }
        }

        public async Task AddFunctionsFromPackage(string packageName) {
            var functionNames = await GetPackageFunctionNamesAsync(packageName);

            var funcs = new BlockingCollection<INamedItemInfo>();
            _packageToFunctionsMap[packageName] = funcs;

            foreach (var functionName in functionNames) {
                _functionToPackageMap[functionName] = packageName;
                funcs.Add(new FunctionInfo(functionName));
            }
        }

        private async Task AddFunctionsFromPackages(IEnumerable<string> packageNames) {
            foreach (var packageName in packageNames) {
                await AddFunctionsFromPackage(packageName);
            }
        }

        private async Task<IEnumerable<string>> GetPackageFunctionNamesAsync(string packageName) {
            try {
                var r = await _host.Session.EvaluateAsync<JArray>(Invariant($"as.list(base::getNamespaceExports('{packageName}'))"), REvaluationKind.Normal);
                return r.Select(p => (string)((JValue)p).Value).ToArray();
            } catch (MessageTransportException) { } catch (TaskCanceledException) { } catch (REvaluationException) { }
            return Enumerable.Empty<string>();
        }
    }
}
