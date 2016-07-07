// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Support.Help.Functions {
    /// <summary>
    /// Contains index of function to package improving 
    /// performance of locating package that contains 
    /// the function documentation.
    /// </summary>
    [Export(typeof(IFunctionIndex))]
    public partial class FunctionIndex {
        private readonly ICoreShell _shell;
        private readonly BinaryAsyncLock _buildIndexLock;

        [ImportingConstructor]
        public FunctionIndex(ICoreShell shell) {
            _shell = shell;
            _buildIndexLock = new BinaryAsyncLock();
        }

        public void BuildIndexForPackage(IPackageInfo package) {
            foreach (INamedItemInfo f in package.Functions) {
                AddFunctionToPackage(package.Name, f);
            }
        }

        public async Task BuildIndexAsync() {
            var indexIsBuilt = await _buildIndexLock.WaitAsync();
            if (!indexIsBuilt) {
                try {
                    await TaskUtilities.SwitchToBackgroundThread();
                    var startIndexBuild = DateTime.Now;
                    BuildIndex();
                    Debug.WriteLine("R functions index build time: {0} ms", (DateTime.Now - startIndexBuild).TotalMilliseconds);
                } finally {
                    _buildIndexLock.Release();
                }
            }
        }

        private void BuildIndex() {
            var packageIndex = _shell.ExportProvider.GetExportedValue<IPackageIndex>();
            var packages = packageIndex.Packages;
            foreach (var p in packages) {
                BuildIndexForPackage(p);
            }
        }

        private void AddFunctionToPackage(string packageName, INamedItemInfo function) {
            BlockingCollection<INamedItemInfo> funcs;

            if (!_packageToFunctionsMap.TryGetValue(packageName, out funcs)) {
                funcs = new BlockingCollection<INamedItemInfo>();
                _packageToFunctionsMap[packageName] = funcs;
            }

            _functionToPackageMap[function.Name] = packageName;
            funcs.Add(function);
        }
    }
}
