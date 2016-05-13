// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.R.DataInspection;
using Microsoft.R.Host.Client;
using static Microsoft.R.DataInspection.REvaluationResultProperties;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public interface IObjectDetailsViewerAggregator {
        IObjectDetailsViewer GetViewer(IRValueInfo result);
    }
}
