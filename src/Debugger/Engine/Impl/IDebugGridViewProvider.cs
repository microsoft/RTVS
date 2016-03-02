// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Debugger.Engine {
    public interface IDebugGridViewProvider {
        bool CanShowDataGrid(DebugEvaluationResult evaluationResult);
        void ShowDataGrid(DebugEvaluationResult evaluationResult);
    }
}
