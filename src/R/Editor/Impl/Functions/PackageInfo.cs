// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Editor.Functions {
    /// <summary>
    /// Represents R package installed on user machine
    /// </summary>
    internal sealed class PackageInfo : NamedItemInfo, IPackageInfo {
        /// <summary>
        /// Maps package name to a list of functions in the package.
        /// Used to extract function names and descriptions when
        /// showing list of functions available in the file.
        /// </summary>
        private readonly ConcurrentBag<INamedItemInfo> _functions;
        private readonly IIntellisenseRSession _host;
        private readonly IFileSystem _fs;
        private readonly string _version;
        private bool _saved;

        public PackageInfo(IIntellisenseRSession host, string name, string description, string version) :
            this(host, name, description, version, Enumerable.Empty<string>()) { }

        public PackageInfo(IIntellisenseRSession host, string name, string description, string version, IEnumerable<string> functionNames) :
            base(name, description, NamedItemType.Package) {
            _host = host;
            _fs = _host.Services.FileSystem();
            _version = version;
            _functions = new ConcurrentBag<INamedItemInfo>(functionNames.Select(fn => new FunctionInfo(fn)));
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
                    if (!_fs.DirectoryExists(dir)) {
                        _fs.CreateDirectory(dir);
                    }
                    using (var file = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write)) {
                        using (var sw = new StreamWriter(file)) {
                            foreach (var f in _functions) {
                                sw.WriteLine(f.Name);
                            }
                        }
                    }
                    _saved = true;
                } catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) {
                    _host.Services.Log().Write(LogVerbosity.Normal, MessageCategory.Error, ex.Message);
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
                    var result = await _host.Session.PackagesFunctionsNamesAsync(Name, REvaluationKind.BaseEnv);
                    functions = result.Children<JValue>().Select(v => (string)v.Value);
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
                if (_fs.FileExists(filePath)) {
                    var list = new List<string>();
                    using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                        using (var sr = new StreamReader(file)) {
                            while (!sr.EndOfStream) {
                                list.Add(sr.ReadLine().Trim());
                            }
                        }
                    }
                    return list;
                }
            } catch (IOException) { } catch (UnauthorizedAccessException) { }
            return null;
        }

        private string CacheFilePath => Path.Combine(_host.Services.GetService<IPackageIndex>().CacheFolderPath, Invariant($"{this.Name}_{_version}.functions"));
    }
}
