// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Support.Help.Packages {
    /// <summary>
    /// Implements enumerator of packages that is based
    /// on the particular collection install path.
    /// Package names normally match names of folders
    /// the packages are installed in.
    /// </summary>
    internal class PackageEnumeration : IEnumerable<IPackageInfo> {
        private string _libraryPath;

        public PackageEnumeration(string libraryPath) {
            _libraryPath = libraryPath;
        }

        public IEnumerator<IPackageInfo> GetEnumerator() {
            return new PackageEnumerator(_libraryPath);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }

    class PackageEnumerator : IEnumerator<IPackageInfo> {
        private IEnumerator<string> _directoriesEnumerator;

        public PackageEnumerator(string libraryPath) {
            if (!string.IsNullOrEmpty(libraryPath) && Directory.Exists(libraryPath)) {
                _directoriesEnumerator = Directory.EnumerateDirectories(libraryPath).GetEnumerator();
            } else {
                _directoriesEnumerator = (new List<string>()).GetEnumerator();
            }
        }

        public IPackageInfo Current {
            get {
                string directoryPath = _directoriesEnumerator.Current;
                if (!string.IsNullOrEmpty(directoryPath)) {
                    string name = Path.GetFileName(directoryPath);
                    return new PackageInfo(name, Path.GetDirectoryName(directoryPath));
                }

                return null;
            }
        }

        object IEnumerator.Current {
            get { return this.Current; }
        }

        public bool MoveNext() {
            return _directoriesEnumerator.MoveNext();
        }

        public void Reset() {
            _directoriesEnumerator.Reset();
        }

        public void Dispose() {
            if (_directoriesEnumerator != null) {
                _directoriesEnumerator.Dispose();
                _directoriesEnumerator = null;
            }
        }
    }
}
