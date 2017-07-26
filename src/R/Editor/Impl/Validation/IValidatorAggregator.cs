// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Validation.Errors;

namespace Microsoft.R.Editor.Validation {
    public interface IValidatorAggregator {
        Task RunAsync(AstRoot ast, bool projectedBuffer, bool linterEnabled, ConcurrentQueue<IValidationError> results, CancellationToken ct);
        bool Busy { get; }
    }
}
