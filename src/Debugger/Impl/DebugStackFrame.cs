// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Debugger {
    internal enum DebugStackFrameKind {
        Normal,
        DoTrace, // .doTrace(if (rtvs:::is_breakpoint(...)) browser())
    }

    /// <summary>
    /// Describes an R stack frame.
    /// </summary>
    /// <remarks>
    /// Properties of this class represent the state of the frame as it was at the point <seealso cref="DebugSession.GetStackFramesAsync"/> was called,
    /// and do not necessarily correspond to the current state if it has changed since then.
    /// </remarks>
    public class DebugStackFrame {
        private static readonly Regex _doTraceRegex = new Regex(
            @"^\.doTrace\(.*rtvs:::is_breakpoint\((?<filename>.*),\s*(?<line_number>\d+)\).*\)$",
            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

        public DebugSession Session { get; }

        /// <summary>
        /// Index of this frame in the call stack. Frames are counted from the bottom of the stack upward,
        /// starting from 0 (which is normally <c>.GlobalEnv</c>).
        /// </summary>
        /// <remarks>
        /// Corresponds to the <c>which</c> parameter of <c>sys.frame()</c> R function.
        /// </remarks>
        public int Index { get; }

        /// <summary>
        /// An R expression that, when evaluated, will produce 
        /// </summary>
        public string EnvironmentExpression => Invariant($"base::sys.frame({Index})");

        /// <summary>
        /// Name of R environment corresponding to this stack frame, or <c>null</c> if it doesn't have a name.
        /// </summary>
        /// <remarks>
        /// This is <em>not</em> the same as <c>environmentName()</c> function in R. Rather, it corresponds to <c>format()</c>.
        /// For example, the name of the global environment is <c>"&lt;environment: R_GlobalEnv&gt;"</c>, and the name of the
        /// base package environment is <c>"&lt;environment: package:utils&gt;"</c>.
        /// </remarks>
        public string EnvironmentName { get; }

        /// <summary>
        /// The stack frame that contains the call that produced this frame (i.e. the frame right below this one on the stack).
        /// </summary>
        public DebugStackFrame CallingFrame { get; }

        /// <summary>
        /// Name of the file in which the currently executing line of code in this frame is located, or <c>null</c> if this
        /// information is not available.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Line number (1-based) of currently executing line of code in this frame is located, or <c>null</c> if this
        /// information is not available.
        /// </summary>
        public int? LineNumber { get; }

        /// <summary>
        /// Currently executing call in this frame, in its string representation as produced by <c>deparse()</c>.
        /// </summary>
        /// <remarks>
        /// Corresponds to <c>sys.call()</c> for the <see cref="Index"/> of the frame.
        /// </remarks>
        public string Call { get; }

        /// <summary>
        /// Whether this frame's environment is <c>.GlobalEnv</c>.
        /// </summary>
        public bool IsGlobal => EnvironmentName == "<environment: R_GlobalEnv>";

        internal DebugStackFrameKind FrameKind { get; }

        internal DebugStackFrame(DebugSession session, int index, DebugStackFrame callingFrame, JObject jFrame, DebugStackFrame fallbackFrame = null) {
            Session = session;
            Index = index;
            CallingFrame = callingFrame;

            FileName = jFrame.Value<string>("filename");
            LineNumber = jFrame.Value<int?>("line_number");
            Call = jFrame.Value<string>("call");
            EnvironmentName = jFrame.Value<string>("env_name");

            if (Call != null) {
                var match = _doTraceRegex.Match(Call);
                if (match.Success) {
                    FrameKind = DebugStackFrameKind.DoTrace;
                }
            }

            if (fallbackFrame != null) {
                // If we still don't have the filename and line number, use those from the fallback frame.
                // This happens during breakpoint hit processing after the context is unwound from within
                // .doTrace back to the function that called it - because we no longer have .doTrace call,
                // we don't have the file/line information that came from it. But DebugSession will have
                // stashed it away when it had it, and then pass it as a fallback frame if index matches.
                FileName = FileName ?? fallbackFrame.FileName;
                LineNumber = LineNumber ?? fallbackFrame.LineNumber;
            }
        }

        /// <summary>
        /// Same as <see cref="DebugSession.EvaluateAsync(string, string, string, DebugEvaluationResultFields, int?, CancellationToken)"/>,
        /// but passes <see cref="EnvironmentExpression"/> of this frame as <c>environmentExpression</c> argument,
        /// </summary>
        public Task<DebugEvaluationResult> EvaluateAsync(
            string expression,
            string name,
            DebugEvaluationResultFields fields,
            string repr = null,
            CancellationToken cancellationToken = default(CancellationToken)
        ) =>
            Session.EvaluateAsync(EnvironmentExpression, expression, name, fields, repr, cancellationToken);

        /// <summary>
        /// Same as <see cref="EvaluateAsync(string, DebugEvaluationResultFields, int?, CancellationToken) "/>,
        /// but uses <see langword="null"/> for <c>name</c>.
        /// </summary>
        public Task<DebugEvaluationResult> EvaluateAsync(
            string expression,
            DebugEvaluationResultFields fields,
            string repr = null,
            CancellationToken cancellationToken = default(CancellationToken)
        ) =>
            EvaluateAsync(expression, null, fields, repr, cancellationToken);

        /// <summary>
        /// Produces an evaluation result describing this frame's environment. Its <see cref="DebugValueEvaluationResult.GetChildrenAsync"/>
        /// method can then be used to retrieve the variables in the frame.
        /// </summary>
        /// <param name="fields">
        /// Which fields of the evaluation result should be provided. Note that it should include at least the flags specified in the default
        /// argument value in order for <see cref="DebugValueEvaluationResult.GetChildrenAsync"/> to be working. 
        /// </param>
        /// <remarks>
        /// <para>
        /// There is no guarantee that the returned evaluation result is <see cref="DebugValueEvaluationResult"/>. Retrieving the frame
        /// environment involves evaluating <see cref="EnvironmentExpression"/>, and like any evaluation, it can fail. Caller should check for
        /// <see cref="DebugErrorEvaluationResult"/> and handle it accordingly. However, it is never <see cref="DebugPromiseEvaluationResult"/>
        /// or <see cref="DebugActiveBindingEvaluationResult"/>.
        /// </para>
        /// <para>
        /// If this method is called on a stale frame (i.e if call stack has changed since the <see cref="DebugSession.GetStackFramesAsync"/>
        /// call that produced this frame), the result is undefined, and can be an error result, or contain unrelated data.
        /// </para>
        /// </remarks>
        public Task<DebugEvaluationResult> GetEnvironmentAsync(
            DebugEvaluationResultFields fields = DebugEvaluationResultFields.Expression | DebugEvaluationResultFields.Length | DebugEvaluationResultFields.AttrCount | DebugEvaluationResultFields.Flags,
            CancellationToken cancellationToken = default(CancellationToken)
        ) =>
            EvaluateAsync("base::environment()", fields: fields, cancellationToken: cancellationToken);

        public override string ToString() =>
            Invariant($"{EnvironmentName ?? Call ?? "<null>"} at {FileName ?? "<null>"}:{(LineNumber?.ToString(CultureInfo.InvariantCulture) ?? "<null>")}");

        public override bool Equals(object obj) =>
            base.Equals(obj) || (obj as IEquatable<DebugStackFrame>)?.Equals(this) == true;

        public override int GetHashCode() =>
            base.GetHashCode();
    }
}
