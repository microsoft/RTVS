using System;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Definitions {
    public interface IPackage: IServiceProvider {
        T GetPackageService<T>(Type t = null) where T : class;
        DialogPage GetDialogPage(Type t);
    }
}
