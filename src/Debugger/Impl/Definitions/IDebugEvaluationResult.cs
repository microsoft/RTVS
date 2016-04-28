// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Debugger {
    public interface IDebugEvaluationResult {
        DebugSession Session { get; }

        /// <summary>
        /// R expression designating the environment in which the evaluation that produced this result took place.
        /// </summary>
        string EnvironmentExpression { get; }

        /// <summary>
        /// R expression that was evaluated to produce this result.
        /// </summary>
        string Expression { get; }

        /// <summary>
        /// Name of the result. This corresponds to the <c>name</c> parameter of <see cref="DebugSession.EvaluateAsync"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is filled automatically when the result is produced by <see cref="DebugValueEvaluationResult.GetChildrenAsync"/>, 
        /// and is primarily useful in that scenario. See the documentation of that method for more information.
        /// </para>
        string Name { get; }
    }
}
