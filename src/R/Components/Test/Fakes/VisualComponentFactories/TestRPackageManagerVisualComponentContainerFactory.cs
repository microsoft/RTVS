// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InfoBar;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.PackageManager.Implementation;
using Microsoft.R.Components.Search;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.View;

namespace Microsoft.R.Components.Test.Fakes.VisualComponentFactories {
    [ExcludeFromCodeCoverage]
    [Export(typeof (IRPackageManagerVisualComponentContainerFactory))]
    internal sealed class TestRPackageManagerVisualComponentContainerFactory : ContainerFactoryBase<IRPackageManagerVisualComponent>, IRPackageManagerVisualComponentContainerFactory {
        private readonly IRSettings _settings;
        private readonly ICoreShell _coreShell;
        private readonly ISearchControlProvider _searchControlProvider;
        private readonly IInfoBarProvider _infoBarProvider;

        [ImportingConstructor]
        public TestRPackageManagerVisualComponentContainerFactory(ISearchControlProvider searchControlProvider, IInfoBarProvider infoBarProvider, IRSettings settings, ICoreShell coreShell) {
            _searchControlProvider = searchControlProvider;
            _infoBarProvider = infoBarProvider;
            _settings = settings;
            _coreShell = coreShell;
        }

        public IVisualComponentContainer<IRPackageManagerVisualComponent> GetOrCreate(IRPackageManager packageManager, int instanceId = 0) 
            => GetOrCreate(instanceId, container => new RPackageManagerVisualComponent(packageManager, container, _searchControlProvider, _infoBarProvider, _settings, _coreShell));
    }
}