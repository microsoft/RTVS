// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using Microsoft.Common.Core.Services;
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
        private readonly IServiceContainer _services;

        public MefCompositionTests(IServiceContainer service) {
            _services = service;
        }

        [Test]
        public void SearchControlProvider() {
            _services.GetService<ISearchControlProvider>().Should().NotBeNull();
        }

        [Test]
        public void RHistoryProvider() {
            _services.GetService<IRHistoryProvider>().Should().NotBeNull();
        }

        [Test]
        public void RInteractiveWorkflowProvider() {
            _services.GetService<IRInteractiveWorkflowProvider>().Should().NotBeNull();
        }

        [Test]
        public void RHistoryVisualComponentContainerFactory() {
            _services.GetService<IRHistoryVisualComponentContainerFactory>().Should().NotBeNull();
        }

        [Test]
        public void InteractiveWindowComponentContainerFactory() {
            _services.GetService<IInteractiveWindowComponentContainerFactory>().Should().NotBeNull();
        }

        [Test]
        public void RPackageManagerVisualComponentContainerFactory() {
            _services.GetService<IRPackageManagerVisualComponentContainerFactory>().Should().NotBeNull();
        }

        [Test]
        public void ConnectionManagerVisualComponentContainerFactory() {
            _services.GetService<IConnectionManagerVisualComponentContainerFactory>().Should().NotBeNull();
        }

        [Test]
        public void InteractiveWindowFactoryService() {
            _services.GetService<IInteractiveWindowFactoryService>().Should().NotBeNull();
        }
    }
}
