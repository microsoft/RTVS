// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.DataInspection;

namespace Microsoft.R.Debugger {
    public interface IDebugGridViewProvider {
        bool CanShowDataGrid(IREvaluationResultInfo evaluationResult);
        void ShowDataGrid(IREvaluationResultInfo evaluationResult);
    }
}
