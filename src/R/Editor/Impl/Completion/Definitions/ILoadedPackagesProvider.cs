using System.Collections.Generic;

namespace Microsoft.R.Editor.Completion.Definitions {
    /// <summary>
    /// Provides list of R packages loaded into the R workspace
    /// Exported via MEF.
    /// </summary>
    public interface ILoadedPackagesProvider {
        IEnumerable<string> GetPackageNames();
    }
}
