using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.R.Components.PackageManager.Implementation.View.DesignTime {
    internal class DesignTimeRPackageInfoListDataSource {
        public IList<object> Items { get; } = new ObservableCollection<object> {
            new DesignTimeRPackageViewModel("abbyyR", "0.3", "0.3", "abc.data, nnet, quantreg, MASS, locfit", "GPL (>= 3)", true, false),
            new DesignTimeRPackageViewModel("abctools", "1.0.4", "1.0.0", "abc, abind, parallel, plyr", "GPL (>= 2)", true, true),
            new DesignTimeRPackageViewModel("ggplot2", "2.1.0", null, "digest, grid, gtable (>= 0.1.1), MASS, plyr (>= 1.7.1), reshape2, scales (>= 0.3.0), stats", "GPL-2", false, false)
        };
    }
}
