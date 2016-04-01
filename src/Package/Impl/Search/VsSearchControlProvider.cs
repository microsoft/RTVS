using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Common.Core;
using Microsoft.R.Components.PackageManager.ViewModel;
using Microsoft.R.Components.Search;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Search {
    [Export(typeof(ISearchControlProvider))]
    internal class VsSearchControlProvider : ISearchControlProvider {
        private readonly Lazy<IVsWindowSearchHostFactory> _factoryLazy = new Lazy<IVsWindowSearchHostFactory>(() => VsAppShell.Current.GetGlobalService<IVsWindowSearchHostFactory>(typeof(SVsWindowSearchHostFactory)));

        public ISearchControl Create(FrameworkElement host, IRPackageManagerViewModel handler, SearchControlSettings settings) {
            VsAppShell.Current.AssertIsOnMainThread();
            var vsWindowSearchHost = _factoryLazy.Value.CreateWindowSearchHost(host);
            return new VsSearchControl(vsWindowSearchHost, handler, settings);
        }
    }
}
