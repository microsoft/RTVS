// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client.Host; // RHostDisconnectedException

namespace Microsoft.R.Host.Client {
    /// <summary>
    /// Represents running session or connection to the process
    /// that hosts R environment (engine).
    /// </summary>
    public interface IRHostSession : IDisposable {
        /// <summary>
        /// Fires when R Host process has started. It is not fully initialized yet. 
        /// Await on <see cref="HostStarted"/> task to detect when host is fully initialized.
        /// </summary>
        event EventHandler<EventArgs> Connected;

        /// <summary>
        /// Fires when host has been terminated. It may have crashed or exited normally.
        /// </summary>
        event EventHandler<EventArgs> Disconnected;

        /// <summary>
        /// Awaitable task that completes when R host process has finished initialization.
        /// </summary>
        Task HostStarted { get; }

        /// <summary>
        /// Indicates if R host process is currently running.
        /// </summary>
        bool IsHostRunning { get; }

        /// <summary>
        /// Indicates if R session is remote.
        /// </summary>
        bool IsRemote { get; }

        /// <summary>
        /// Starts R host process.
        /// </summary>
        /// <param name="callback">
        /// A set of callbacks that are called when R engine requests certain operation
        /// that are usually provided by the application
        /// </param>
        /// <param name="workingDirectory">R working directory</param>
        /// <param name="codePage">R code page to set</param>
        /// <param name="timeout">Timeout to wait for the host process to start</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        Task StartHostAsync(IRHostSessionCallback callback, string workingDirectory = null, int codePage = 0, int timeout = 3000, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Stops R host process
        /// </summary>
        /// <param name="waitForShutdown">
        /// If true, the method will wait for the R Host process to exit.
        /// If false, the process will receive termination request and the call will return immediately.
        /// </param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        Task StopHostAsync(bool waitForShutdown = true, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Attempts to cancel all running tasks in the R Host. 
        /// This is similar to 'Interrupt R' command.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        Task CancelAllAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Executes R code
        /// </summary>
        /// <param name="expression">Expression or block of R code to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        Task ExecuteAsync(string expression, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Executes R code and returns output as it would appear in the interactive window.
        /// </summary>
        /// <param name="expression">Expression or block of R code to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        Task<RSessionOutput> ExecuteAndOutputAsync(string expression, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Evaluates the provided expression and returns the result.
        /// This method is typically used to fetch variable value and return it to .NET code.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the variable expected. This must be a simple type.
        /// To return collections use <see cref="GetListAsync"/> and <see cref="GetDataFrameAsync"/>
        /// </typeparam>
        /// <param name="expression">Expression or block of R code to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The variable or expression value</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        Task<T> EvaluateAsync<T>(string expression, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Invokes R function with a set of arguments. Does not return any value.
        /// </summary>
        /// <param name="function">Function name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="args">Function arguments</param>
        /// <exception cref="ArgumentException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        Task InvokeAsync(string function, CancellationToken cancellationToken = default(CancellationToken), params object[] args);

        /// <summary>
        /// Invokes R function with a set of arguments. Returns name of a 
        /// temporary variable that received the result.
        /// </summary>
        /// <param name="function">Function name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="args">Function arguments</param>
        /// <returns>Name of the variable that holds the data returned by the function</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        Task<string> InvokeAndReturnAsync(string function, CancellationToken cancellationToken = default(CancellationToken), params object[] args);

        /// <summary>
        /// Retrieves list of unknown type from R variable
        /// </summary>
        /// <param name="expression">Expression (variable name) to fetch as list</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of objects</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        Task<List<object>> GetListAsync(string expression, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Retrieves list of specific type from R variable
        /// </summary>
        /// <param name="expression">Expression (variable name) to fetch as list</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of values of the provided type</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        Task<List<T>> GetListAsync<T>(string expression, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Retrieves data frame from R variable
        /// </summary>
        /// <param name="expression">Expression (variable name) to fetch as data frame</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Data frame</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        Task<DataFrame> GetDataFrameAsync(string expression, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Retrieves information about R object or expression
        /// </summary>
        /// <param name="expression">Expression (variable name) to describe</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object information</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        Task<IRObjectInformation> GetInformationAsync(string expression, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Passes expression the the R plot function and returns plot image data.
        /// </summary>
        /// <param name="expression">Expression or variable name to plot</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="dpi">Image resolution</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Image data</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        Task<byte[]> PlotAsync(string expression, int width, int height, int dpi, CancellationToken cancellationToken = default(CancellationToken));
    }
}
