// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
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
        private readonly ICoreShell _shell;
        private readonly IIntellisenseRHost _host;
        private readonly IPackageIndex _packageIndex;
        private readonly BinaryAsyncLock _buildIndexLock;

        [ImportingConstructor]
        public FunctionIndex(ICoreShell shell, IPackageIndex packageIndex, IIntellisenseRHost hostSession, IFunctionRdDataProvider rdDataProfider) {
            _shell = shell;
            _host = hostSession;
            _packageIndex = packageIndex;
            _functionRdDataProvider = rdDataProfider;
            _buildIndexLock = new BinaryAsyncLock();
        }

        public bool IsReady { get; private set; }
        public IRSession RSession => _host.Session;

        public async Task BuildIndexAsync() {
            IsReady = await _buildIndexLock.WaitAsync();
            if (!IsReady) {
                try {
                    await TaskUtilities.SwitchToBackgroundThread();
                    await _host.CreateSessionAsync();

                    var startIndexBuild = DateTime.Now;

                    await _packageIndex.BuildIndexAsync(this);
                    await AddFunctionsFromAllPackages();
                    IsReady = true;

                    Debug.WriteLine("R functions index build time: {0} ms", (DateTime.Now - startIndexBuild).TotalMilliseconds);
                } finally {
                    _buildIndexLock.Release();
                }
            }
        }

        private async Task AddFunctionsFromAllPackages() {
            var packages = _packageIndex.Packages;
            foreach (var p in packages) {
                var names = await GetPackageFunctionNamesAsync(p.Name);

                var funcs = new BlockingCollection<INamedItemInfo>();
                _packageToFunctionsMap[p.Name] = funcs;

                foreach (var functionName in names) {
                    _functionToPackageMap[functionName] = p.Name;
                    funcs.Add(new FunctionInfo(functionName));
                }
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
