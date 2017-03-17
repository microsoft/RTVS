// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Windows;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Search;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Search {
    [Export(typeof(ISearchControlProvider))]
    internal class VsSearchControlProvider : ISearchControlProvider {
        private readonly Lazy<IVsWindowSearchHostFactory> _factoryLazy = new Lazy<IVsWindowSearchHostFactory>(() 
            => VsAppShell.Current.GetService<IVsWindowSearchHostFactory>(typeof(SVsWindowSearchHostFactory)));

        public ISearchControl Create(FrameworkElement host, ISearchHandler handler, SearchControlSettings settings) {
            VsAppShell.Current.AssertIsOnMainThread();

            var vsWindowSearchHost = _factoryLazy.Value.CreateWindowSearchHost(host);
            return new VsSearchControl(vsWindowSearchHost, handler, settings);
        }
    }
}
