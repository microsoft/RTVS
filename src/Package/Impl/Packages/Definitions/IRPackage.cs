using System;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Definitions {
    internal interface IRPackage : IPackage {
        T FindWindowPane<T>(Type t, int id, bool create) where T : ToolWindowPane;
    }
}
