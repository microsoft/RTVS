// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.Logging;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Help.Functions;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Support.Help.Packages {
    /// <summary>
    /// Represents R package installed on user machine
    /// </summary>
    internal sealed class PackageInfo : NamedItemInfo, IPackageInfo {
        /// <summary>
        /// Maps package name to a list of functions in the package.
        /// Used to extract function names and descriptions when
        /// showing list of functions available in the file.
        /// </summary>
        private readonly BlockingCollection<INamedItemInfo> _functions = new BlockingCollection<INamedItemInfo>();
        private readonly IIntellisenseRHost _host;
        private readonly string _version;

        public PackageInfo(IIntellisenseRHost host, string name, string description, string version) :
            base(name, description, NamedItemType.Package) {
            _host = host;
            _version = version;
        }

        #region IPackageInfo
        /// <summary>
        /// Collection of functions in the package
        /// </summary>
        public IEnumerable<INamedItemInfo> Functions => _functions;
        #endregion

        public void Dispose() {
            SaveFunctionsList();
        }

        public async Task LoadFunctionsIndexAsync() {
            var functionNames = await GetFunctionNamesAsync();
            foreach (var functionName in functionNames) {
                _functions.Add(new FunctionInfo(functionName));
            }
            _functions.CompleteAdding();
        }

        private async Task<IEnumerable<string>> GetFunctionNamesAsync() {
            var cached = TryRestoreFromCache();
            if (cached == null) {
                try {
                    var r = await _host.Session.EvaluateAsync<JArray>(Invariant($"as.list(base::getNamespaceExports('{this.Name}'))"), REvaluationKind.Normal);
                    return r.Select(p => (string)((JValue)p).Value).ToArray();
                } catch (MessageTransportException) { } catch (TaskCanceledException) { } catch (REvaluationException) { }
            }
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Attempts to locate cached function list for the package
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> TryRestoreFromCache() {
            var filePath = GetCacheFilePath();
            try {
                var list = new List<string>();
                using (var sr = new StreamReader(filePath)) {
                    while (!sr.EndOfStream) {
                        list.Add(sr.ReadLine().Trim());
                    }
                }
                return list;
            } catch (IOException) { } catch (AccessViolationException) { }

            return null;
        }

        public void SaveFunctionsList() {
            var filePath = GetCacheFilePath();
            try {
                using (var sw = new StreamWriter(filePath)) {
                    foreach (var f in _functions) {
                        sw.WriteLine(f.Name);
                    }
                }
            } catch (IOException ioex) {
                GeneralLog.Write(ioex);
            } catch (AccessViolationException aex) {
                GeneralLog.Write(aex);
            }
        }

        private string GetCacheFilePath() {
            var folder = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            @"Microsoft\RTVS\IntelliSense\");
            return Path.Combine(folder, Invariant($"{this.Name}_{_version}.functions"));
        }
    }
}
