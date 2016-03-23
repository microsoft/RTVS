// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Debugger;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Definitions {
    /// <summary>
    /// provides evaluation from R Host
    /// </summary>
    public interface IVariableDataProvider {
        /// <summary>
        /// register a callback when evaluation is available
        /// </summary>
        /// <param name="frameIndex">frame index to evaluation the expression</param>
        /// <param name="expression">expression to evaluate</param>
        /// <param name="executeAction">callback when evaluation is avilable</param>
        /// <returns>a subscription</returns>
        VariableSubscription Subscribe(int frameIndex, string expression, Action<DebugEvaluationResult> executeAction);

        /// <summary>
        /// unregister the subscription
        /// </summary>
        /// <param name="subscription">the subscription to quit</param>
        void Unsubscribe(VariableSubscription subscription);

        /// <summary>
        /// indicates that Variable can be provided through R session
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Returns evaluation result
        /// </summary>
        /// <param name="expression">expression to run</param>
        /// <returns></returns>
        EvaluationWrapper Evaluate(string expression);
    }
}
