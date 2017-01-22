// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Languages.Editor.Shell;
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
        private readonly ConcurrentBag<INamedItemInfo> _functions = new ConcurrentBag<INamedItemInfo>();
        private readonly IIntellisenseRSession _host;
        private readonly string _version;
        private bool _saved;

        public PackageInfo(IIntellisenseRSession host, string name, string description, string version) :
            base(name, description, NamedItemType.Package) {
            _host = host;
            _version = version;
        }

        #region IPackageInfo
        /// <summary>
        /// Collection of functions in the package
        /// </summary>
        public IEnumerable<INamedItemInfo> Functions => _functions;

        public void WriteToDisk() {
            if (!_saved) {
                var filePath = CacheFilePath;
                try {
                    var dir = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(dir)) {
                        Directory.CreateDirectory(dir);
                    }
                    using (var sw = new StreamWriter(filePath)) {
                        foreach (var f in _functions) {
                            sw.WriteLine(f.Name);
                        }
                    }
                    _saved = true;
                } catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) {
                    EditorShell.Current.Services.Log.Write(LogVerbosity.Normal, MessageCategory.Error, ex.Message);
                }
            }
        }
        #endregion

        public async Task LoadFunctionsIndexAsync() {
            var functionNames = await GetFunctionNamesAsync();
            foreach (var functionName in functionNames) {
                _functions.Add(new FunctionInfo(functionName));
            }
        }

        private async Task<IEnumerable<string>> GetFunctionNamesAsync() {
            var functions = TryRestoreFromCache();
            if (functions == null || !functions.Any()) {
                try {
                    var result = await _host.Session.EvaluateAsync<JArray>(Invariant($"as.list(getNamespaceExports('{this.Name}'))"), REvaluationKind.BaseEnv);
                    functions = result
                                    .Select(p => (string)((JValue)p).Value)
                                    .Where(n => n.IndexOf(':') < 0);
                    result = await _host.Session.EvaluateAsync<JArray>(Invariant($"as.list(ls('package:{this.Name}'))"), REvaluationKind.BaseEnv);
                    var variables = result
                                    .Select(p => (string)((JValue)p).Value)
                                    .Where(n => n.IndexOf(':') < 0);

                    functions = functions.Union(variables);
                } catch (TaskCanceledException) { } catch (REvaluationException) { }
            } else {
                _saved = true;
            }
            return functions ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// Attempts to locate cached function list for the package
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> TryRestoreFromCache() {
            var filePath = this.CacheFilePath;
            try {
                if (File.Exists(filePath)) {
                    var list = new List<string>();
                    using (var sr = new StreamReader(filePath)) {
                        while (!sr.EndOfStream) {
                            list.Add(sr.ReadLine().Trim());
                        }
                    }
                    return list;
                }
            } catch (IOException) { } catch (UnauthorizedAccessException) { }

            return null;
        }

        private string CacheFilePath => Path.Combine(PackageIndex.CacheFolderPath, Invariant($"{this.Name}_{_version}.functions"));
    }
}
