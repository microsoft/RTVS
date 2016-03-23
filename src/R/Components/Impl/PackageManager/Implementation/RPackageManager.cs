// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.PackageManager.ViewModel;

namespace Microsoft.R.Components.PackageManager.Implementation {
    internal class RPackageManager : IRPackageManager {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly Action _dispose;

        public IRPackageManagerVisualComponent VisualComponent { get; private set; }
        public IRPackageManagerViewModel _viewModel;

        public RPackageManager(IRInteractiveWorkflow interactiveWorkflow, Action dispose) {
            _interactiveWorkflow = interactiveWorkflow;
            _dispose = dispose;
        }

        public IRPackageManagerVisualComponent GetOrCreateVisualComponent(IRPackageManagerVisualComponentContainerFactory visualComponentContainerFactory, int instanceId = 0) {
            if (VisualComponent != null) {
                return VisualComponent;
            }

            VisualComponent = visualComponentContainerFactory.GetOrCreate(this, instanceId).Component;
            return VisualComponent;
        }

        public void Dispose() {
            _dispose();
        }
    }
}
