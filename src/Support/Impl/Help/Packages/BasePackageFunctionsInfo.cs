using System.Collections.Generic;
using Microsoft.R.Support.Utility;

namespace Microsoft.R.Support.Help.Packages
{
    public sealed class BasePackageFunctionsInfo : AsyncDataSource<IReadOnlyList<string>>
    {
        public BasePackageFunctionsInfo(string name): base(name, PackagesDataSource.)
        {
            _packageName = name;
        }
    }
}
