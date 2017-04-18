// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.PackageManager.Implementation;
using Microsoft.R.Components.Search;
using Microsoft.R.Components.View;

namespace Microsoft.R.Components.Test.Fakes.VisualComponentFactories {
    [ExcludeFromCodeCoverage]
    [Export(typeof (IRPackageManagerVisualComponentContainerFactory))]
    internal sealed class TestRPackageManagerVisualComponentContainerFactory : ContainerFactoryBase<IRPackageManagerVisualComponent>, IRPackageManagerVisualComponentContainerFactory {
        private readonly ICoreShell _coreShell;
        private readonly ISearchControlProvider _searchControlProvider;

        [ImportingConstructor]
        public TestRPackageManagerVisualComponentContainerFactory(ISearchControlProvider searchControlProvider, ICoreShell coreShell) {
            _searchControlProvider = searchControlProvider;
            _coreShell = coreShell;
        }

        public IVisualComponentContainer<IRPackageManagerVisualComponent> GetOrCreate(IRPackageManager packageManager, int instanceId = 0) 
            => GetOrCreate(instanceId, container => new RPackageManagerVisualComponent(packageManager, container, _searchControlProvider, _coreShell.Services));
    }
}