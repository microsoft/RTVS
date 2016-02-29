// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.Plots.Definitions {
    internal interface IPlotHistoryProvider {
        IPlotHistory GetPlotHistory(IRSession session);
    }
}
