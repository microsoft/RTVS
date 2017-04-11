// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.Search;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.InteractiveWindow;

namespace Microsoft.R.Components.Test {
    /// <summary>
    /// These tests are basic markers that all required services are available.
    /// </summary>
    public class MefCompositionTests {
        private readonly ICoreShell _coreShell;

        public MefCompositionTests(RComponentsShellProviderFixture shellProvider) {
            _coreShell = shellProvider.CoreShell;
        }

        [Test]
        public void SearchControlProvider() {
            _coreShell.GetService<ISearchControlProvider>().Should().NotBeNull();
        }

        [Test]
        public void RHistoryProvider() {
            _coreShell.GetService<IRHistoryProvider>().Should().NotBeNull();
        }

        [Test]
        public void RInteractiveWorkflowProvider() {
            _coreShell.GetService<IRInteractiveWorkflowProvider>().Should().NotBeNull();
        }

        [Test]
        public void RHistoryVisualComponentContainerFactory() {
            _coreShell.GetService<IRHistoryVisualComponentContainerFactory>().Should().NotBeNull();
        }

        [Test]
        public void InteractiveWindowComponentContainerFactory() {
            _coreShell.GetService<IInteractiveWindowComponentContainerFactory>().Should().NotBeNull();
        }

        [Test]
        public void RPackageManagerVisualComponentContainerFactory() {
            _coreShell.GetService<IRPackageManagerVisualComponentContainerFactory>().Should().NotBeNull();
        }

        [Test]
        public void ConnectionManagerVisualComponentContainerFactory() {
            _coreShell.GetService<IConnectionManagerVisualComponentContainerFactory>().Should().NotBeNull();
        }

        [Test]
        public void InteractiveWindowFactoryService() {
            _coreShell.GetService<IInteractiveWindowFactoryService>().Should().NotBeNull();
        }
    }
}
