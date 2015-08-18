using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.R.Support.Help;
using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Support.Help.Packages
{
    class PackageEnumeration : IEnumerable<IPackageInfo>
    {
        private string _libraryPath;
        private bool _isBase;

        public PackageEnumeration(string libraryPath, bool isBase)
        {
            _libraryPath = libraryPath;
            _isBase = isBase;
        }

        public IEnumerator<IPackageInfo> GetEnumerator()
        {
            return new PackageEnumerator(_libraryPath, _isBase);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    class PackageEnumerator : IEnumerator<IPackageInfo>
    {
        private IEnumerator<string> _directoriesEnumerator;
        private bool _isBase;

        public PackageEnumerator(string libraryPath, bool isBase)
        {
            _isBase = isBase;
            _directoriesEnumerator = Directory.EnumerateDirectories(libraryPath).GetEnumerator();
        }

        public IPackageInfo Current
        {
            get
            {
                string directoryPath = _directoriesEnumerator.Current;
                string name = Path.GetFileName(directoryPath).ToLowerInvariant();

                PackageInfo packageInfo = new PackageInfo(name, Path.GetDirectoryName(directoryPath));
                packageInfo.IsBase = _isBase;

                return packageInfo;
            }
        }

        object IEnumerator.Current
        {
            get { return this.Current; }
        }

        public bool MoveNext()
        {
            return _directoriesEnumerator.MoveNext();
        }

        public void Reset()
        {
            _directoriesEnumerator.Reset();
        }

        public void Dispose()
        {
        }
    }
}
