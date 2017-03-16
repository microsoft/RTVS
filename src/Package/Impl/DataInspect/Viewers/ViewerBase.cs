// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.DataInspection;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    public abstract class ViewerBase {
        protected IDataObjectEvaluator Evaluator { get; }

        protected ViewerBase(IDataObjectEvaluator evaluator) {
             Evaluator = evaluator;
        }

        protected async Task<IRValueInfo> EvaluateAsync(string expression, REvaluationResultProperties fields, string repr, CancellationToken cancellationToken) {
            var result = await Evaluator.EvaluateAsync(expression, fields, repr, cancellationToken);
            var error = result as IRErrorInfo;
            if (error != null) {
                await VsAppShell.Current.SwitchToMainThreadAsync(cancellationToken);
                VsAppShell.Current.ShowErrorMessage(error.ErrorText);
                return null;
            }

            return result as IRValueInfo;
        }
    }
}
