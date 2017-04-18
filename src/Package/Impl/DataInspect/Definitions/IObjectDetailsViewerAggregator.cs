// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.DataInspection;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public interface IObjectDetailsViewerAggregator {
        IObjectDetailsViewer GetViewer(IRValueInfo result);
    }
}
