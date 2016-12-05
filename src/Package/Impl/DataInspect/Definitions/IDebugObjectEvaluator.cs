// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.DataInspection;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public interface IDataObjectEvaluator {
       Task<IREvaluationResultInfo> EvaluateAsync(string expression, REvaluationResultProperties fields, string repr, CancellationToken cancellationToken = default(CancellationToken));
    }
}
