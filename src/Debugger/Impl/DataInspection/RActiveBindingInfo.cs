// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Host.Client;

namespace Microsoft.R.DataInspection {
    internal sealed class RActiveBindingInfo : REvaluationResultInfo, IRActiveBindingInfo {
        internal RActiveBindingInfo(IRSession session, string environmentExpression, string expression, string name)
            : base(session, environmentExpression, expression, name) {
        }
    }
}
