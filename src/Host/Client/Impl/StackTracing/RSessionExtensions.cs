// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.StackTracing {
    public static class RSessionExtensions {
        /// <summary>
        /// Retrieve the current call stack, in call order (i.e. the current active frame is last, the one that called it is second to last etc).
        /// </summary>
        /// <param name="skipSourceFrames">
        /// If <see langword="true"/>, excludes frames that belong to <c>source()</c> or <c>rtvs:::debug_source()</c> internal machinery at the bottom of the stack;
        /// the first reported frame will be the one with sourced code.
        /// </param>
        /// <remarks>
        /// This method has snapshot semantics for the frames and their properties - that is, the returned collection is not going to change as code runs.
        /// However, calling various methods on the returned <see cref="IRStackFrame"/> objects, such as <see cref="RStackFrameExtensions.DescribeChildrenAsync"/>,
        /// will fetch fresh data, possibly from altogether different frames if the call stack has changed. Thus, it is inadvisable to retain the returned
        /// stack and use it at a later point - it should always be obtained anew at the point where it is used. 
        /// </remarks>
        public static async Task<IReadOnlyList<IRStackFrame>> TracebackAsync(
            this IRSession session,
            bool skipSourceFrames = true,
            CancellationToken cancellationToken = default(CancellationToken)
        ) {
            await TaskUtilities.SwitchToBackgroundThread();

            var jFrames = await session.EvaluateAsync<JArray>("rtvs:::describe_traceback()", REvaluationKind.Normal, cancellationToken);
            Trace.Assert(jFrames.All(t => t is JObject), "rtvs:::describe_traceback(): array of objects expected.\n\n" + jFrames);

            var stackFrames = new List<RStackFrame>();

            RStackFrame lastFrame = null;
            int i = 0;
            foreach (JObject jFrame in jFrames) {
                lastFrame = new RStackFrame(session, i, lastFrame, jFrame);
                stackFrames.Add(lastFrame);
                ++i;
            }

            if (skipSourceFrames) {
                var firstFrame = stackFrames.FirstOrDefault();
                if (firstFrame != null && firstFrame.IsGlobal && firstFrame.Call != null) {
                    if (firstFrame.Call.StartsWith("source(") || firstFrame.Call.StartsWith("rtvs::debug_source(")) {
                        // Skip everything until the first frame that has a line number - that will be the sourced code.
                        stackFrames = stackFrames.SkipWhile(f => f.LineNumber == null).ToList();
                    }
                }
            }

            return stackFrames;
        }
    }
}
