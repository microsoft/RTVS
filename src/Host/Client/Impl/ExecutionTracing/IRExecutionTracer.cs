// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.R.ExecutionTracing {
    /// <summary>
    /// Wraps <see cref="IRSession"/> and provides functionality to step through R code and set breakpoints in it.
    /// </summary>
    /// <remarks>
    /// Use <see cref="RSessionExtensions.TraceExecutionAsync"/> to obtain an object implementing this interface
    /// for a given <see cref="IRSession"/>.
    /// </remarks>
    public interface IRExecutionTracer {
        IReadOnlyCollection<IRBreakpoint> Breakpoints { get; }

        IRSession Session { get; }

        /// <summary>
        /// Raised when the associated R session is stopped at the Browse> prompt.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If a handler is subscribed when the session is already at a Browse> prompt, that handler will be
        /// invoked immediately.
        /// </para>
        /// <para>
        /// If a stepping operation is in progress that requires issuing several consecutive commands, the event is not
        /// raised for any intermediate Browse> prompts, but only for the final prompt at which the step is complete.
        /// </para>
        /// </remarks>
        event EventHandler<RBrowseEventArgs> Browse;

        /// <summary>
        /// Waits for the next REPL prompt, and executes the given command at it if it is a Browse> prompt.
        /// </summary>
        /// <param name="command">Command to execute. The trailing newline is appended automatically.</param>
        /// <param name="prepare">
        /// If not <see langword="null"/>, the provided delegate is invoked after getting exclusive access
        /// to the prompt, but before command is executed. Can be used to perform preparatory evaluations.</param>
        /// <returns>
        /// <see langword="true"/> if command was successfully submitted for execution.
        /// <see langword="false"/> if the next prompt was not a Browse> prompt.
        /// </returns>
        Task<bool> ExecuteBrowserCommandAsync(string command, Func<IRSessionInteraction, Task<bool>> prepare = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Force the R session to pause wherever it is currently executing, with a Browse> prompt.
        /// </summary>
        /// <returns>A task that completes when the prompt appears.</returns>
        Task BreakAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// When paused at a Browse> prompt, continue execution.
        /// </summary>
        Task ContinueAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// When paused at a Browse> prompt, step into the next call.
        /// </summary>
        /// <returns>
        /// A task that completes when the step is either completed or interrupted (e.g. by a breakpoint).
        /// The result is <see langword="true"/> if step was completed, and <see langword="false"/> if it was abandoned.
        /// </returns>
        /// <remarks>
        /// Detailed semantics of step in are described in R documentation for
        /// <a href="https://stat.ethz.ch/R-manual/R-devel/library/base/html/browser.html"><c>browser()</c></a> "s" command.
        /// </remarks>
        Task<bool> StepIntoAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// When paused at a Browse> prompt, step over the next call.
        /// </summary>
        /// <returns>
        /// A task that completes when the step is either completed or interrupted (e.g. by a breakpoint).
        /// The result is <see langword="true"/> if step was completed, and <see langword="false"/> if it was abandoned.
        /// </returns>
        /// <remarks>
        /// Detailed semantics of step over are described in R documentation for
        /// <a href="https://stat.ethz.ch/R-manual/R-devel/library/base/html/browser.html"><c>browser()</c></a> "n" command.
        /// </remarks>
        Task<bool> StepOverAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// When paused at a Browse> prompt, step out from the current call.
        /// </summary>
        /// <returns>
        /// A task that completes when the step is either completed or interrupted (e.g. by a breakpoint).
        /// The result is <see langword="true"/> if step was completed, and <see langword="false"/> if it was abandoned.
        /// </returns>
        /// Detailed semantics of step out are described in R documentation for
        /// <a href="https://stat.ethz.ch/R-manual/R-devel/library/base/html/browserText.html"><c>browserSetDebug()</c></a> function.
        /// </remarks>
        Task<bool> StepOutAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// If a step operation is currently in progress, cancel it.
        /// </summary>
        bool CancelStep();

        /// <summary>
        /// Enables or disables breakpoints. When enabled, <see cref="IRBreakpoint.BreakpointHit"/> events are raised.
        /// </summary>
        Task EnableBreakpointsAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates a new breakpoint at the specified location.
        /// </summary>
        Task<IRBreakpoint> CreateBreakpointAsync(RSourceLocation location, CancellationToken cancellationToken = default(CancellationToken));
    }
}
