// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.R.Host.Client {
    /// <summary>
    /// Representation of <c>struct RCTXT</c> in R.
    /// </summary>
    public interface IRContext {
        RContextType CallFlag { get; }
    }

    public static class RContextExtensions {
        public static bool IsBrowser(this IReadOnlyList<IRContext> contexts) {
            return contexts.SkipWhile(context => context.CallFlag.HasFlag(RContextType.Restart)).FirstOrDefault()?.CallFlag.HasFlag(RContextType.Browser) == true;
        }
    }
}
