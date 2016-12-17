// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.DataInspection;
using Microsoft.R.Host.Client;
using static Microsoft.R.DataInspection.REvaluationResultProperties;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public static class ObjectDetailsViewerAggregatorExtensions {
        public static async Task<IObjectDetailsViewer> GetViewer(this IObjectDetailsViewerAggregator aggregator, IRSession session, string environmentExpression, string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            var preliminary = await session.TryEvaluateAndDescribeAsync(expression, TypeNameProperty | ClassesProperty | DimProperty | LengthProperty, null, cancellationToken) as IRValueInfo;
            return preliminary != null ? aggregator.GetViewer(preliminary) : null;
        }
    }
}
