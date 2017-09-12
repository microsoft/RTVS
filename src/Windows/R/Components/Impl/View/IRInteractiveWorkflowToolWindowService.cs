// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.View {
    public interface IRInteractiveWorkflowToolWindowService {
        IToolWindow Connections(int instanceId = 0);
        IToolWindow Containers(int instanceId = 0);
        IToolWindow Packages(int instanceId = 0);
    }
}