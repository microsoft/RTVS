// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.R.Debugger;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public interface IObjectDetailsViewer {
        bool IsTable { get; }

        DebugEvaluationResultFields EvaluationFields { get; }

        bool CanView(DebugValueEvaluationResult evaluation);

        Task ViewAsync(DebugValueEvaluationResult evaluation, string title);

        Task<object> GetTooltipAsync(DebugValueEvaluationResult evaluation);
    }
}
