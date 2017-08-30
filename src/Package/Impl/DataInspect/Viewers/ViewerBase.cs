// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.R.DataInspection;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    public abstract class ViewerBase {
        protected IDataObjectEvaluator Evaluator { get; }
        protected IServiceContainer Services { get; }
        
        protected ViewerBase(IServiceContainer services, IDataObjectEvaluator evaluator) {
            Services = services;
            Evaluator = evaluator;
        }

        protected async Task<IRValueInfo> EvaluateAsync(string expression, REvaluationResultProperties fields, string repr, CancellationToken cancellationToken) {
            var result = await Evaluator.EvaluateAsync(expression, fields, repr, cancellationToken);

            if (result is IRErrorInfo error) {
                await Services.MainThread().SwitchToAsync(cancellationToken);
                Services.UI().ShowErrorMessage(error.ErrorText);
                return null;
            }
            return result as IRValueInfo;
        }
    }
}
