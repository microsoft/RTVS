// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
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
        private const int Version = 2;

        /// <summary>
        /// Maps package name to a list of functions in the package.
        /// Used to extract function names and descriptions when
        /// showing list of functions available in the file.
        /// </summary>
        private readonly ConcurrentBag<IFunctionInfo> _functions;
        private readonly IIntellisenseRSession _host;
        private readonly IFileSystem _fs;
        private readonly string _version;
        private bool _saved;

        public PackageInfo(IIntellisenseRSession host, string name, string description, string version) :
            this(host, name, description, version, Enumerable.Empty<IPersistentFunctionInfo>()) { }

        public PackageInfo(IIntellisenseRSession host, string name, string description, string version, IEnumerable<IPersistentFunctionInfo> functions) :
            base(name, description, NamedItemType.Package) {
            _host = host;
            _fs = _host.Services.FileSystem();
            _version = version;
            _functions = new ConcurrentBag<IFunctionInfo>(functions.Select(fn => new FunctionInfo(fn.Name, fn.IsInternal)));
        }

        #region IPackageInfo
        /// <summary>
        /// Collection of functions in the package
        /// </summary>
        public IEnumerable<IFunctionInfo> Functions => _functions;

        public void WriteToDisk() {
            if (!_saved) {
                var filePath = CacheFilePath;
                try {
                    var dir = Path.GetDirectoryName(filePath);
                    if (!_fs.DirectoryExists(dir)) {
                        _fs.CreateDirectory(dir);
                    }
                    using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write)) {
                        using (var sw = new StreamWriter(file)) {
                            sw.WriteLine(VersionString);
                            foreach (var f in _functions) {
                                sw.WriteLine(Invariant($"`{f.Name}` {f.IsInternal}"));
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

        public async Task LoadFunctionsIndexAsync(CancellationToken ct) {
            var functions = await GetFunctionNamesAsync(ct);
            foreach (var function in functions) {
                if(ct.IsCancellationRequested) {
                    break;
                }
                _functions.Add(new FunctionInfo(function));
            }
        }

        private async Task<IEnumerable<IPersistentFunctionInfo>> GetFunctionNamesAsync(CancellationToken ct) {
            var functions = TryRestoreFromCache();
            if (functions == null || !functions.Any()) {
                try {
                    var result = await _host.Session.PackageExportedFunctionsNamesAsync(Name, REvaluationKind.BaseEnv, ct);
                    var exportedFunctions = new HashSet<string>(result.Children<JValue>().Select(v => (string)v.Value));

                    result = await _host.Session.PackageAllFunctionsNamesAsync(Name, REvaluationKind.BaseEnv, ct);
                    var allFunctions = result.Children<JValue>().Select(v => (string)v.Value);

                    functions = allFunctions.Select(x => new PersistentFunctionInfo(x, !exportedFunctions.Contains(x)));
                } catch (TaskCanceledException) { } catch (REvaluationException) { }
            } else {
                _saved = true;
            }
            return functions ?? Enumerable.Empty<IPersistentFunctionInfo>();
        }

        /// <summary>
        /// Attempts to locate cached function list for the package
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IPersistentFunctionInfo> TryRestoreFromCache() {
            var filePath = this.CacheFilePath;
            try {
                if (_fs.FileExists(filePath)) {
                    Debug.WriteLine("Restoring function index from cache");
                    var list = new List<IPersistentFunctionInfo>();
                    using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                        using (var sr = new StreamReader(file)) {
                            var s = sr.ReadLine();
                            if(!s.EqualsOrdinal(VersionString)) {
                                return null; // incompatible
                            }
                            while (!sr.EndOfStream) {
                                s = sr.ReadLine().Trim();
                                if(!PersistentFunctionInfo.TryParse(s, out var info)) { 
                                    return null;
                                }
                                list.Add(info);
                            }
                        }
                    }
                    return list;
                }
            } catch (IOException) { } catch (UnauthorizedAccessException) { }
            return null;
        }

        private string CacheFilePath => Path.Combine(_host.Services.GetService<IPackageIndex>().CacheFolderPath, Invariant($"{this.Name}_{_version}.functions"));
        private string VersionString => Invariant($"Version: {Version}");
    }
}
