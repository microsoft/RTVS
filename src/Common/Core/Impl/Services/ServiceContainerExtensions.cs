// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Tasks;
using Microsoft.Common.Core.Telemetry;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Core.UI.Commands;

namespace Microsoft.Common.Core.Services {
    public static class ServiceContainerExtensions {
        public static IActionLog Log(this IServiceContainer sc) => sc.GetService<IActionLog>();
        public static IFileSystem FileSystem(this IServiceContainer sc) => sc.GetService<IFileSystem>();
        public static IProcessServices Process(this IServiceContainer sc) => sc.GetService<IProcessServices>();
        public static ITelemetryService Telemetry(this IServiceContainer sc) => sc.GetService<ITelemetryService>();
        public static ISecurityService Security(this IServiceContainer sc) => sc.GetService<ISecurityService>();
        public static ITaskService Tasks(this IServiceContainer sc) => sc.GetService<ITaskService>();
        public static IUIService UI(this IServiceContainer sc) => sc.GetService<IUIService>();
        public static IMainThread MainThread(this IServiceContainer sc) => sc.GetService<IMainThread>();
        public static IIdleTimeService IdleTime(this IServiceContainer sc) => sc.GetService<IIdleTimeService>();

        /// <summary>
        /// Displays application-specific modal progress window
        /// </summary>
        public static IProgressDialog ProgressDialog(this IServiceContainer sc) => sc.UI().ProgressDialog;

        /// <summary>
        /// Displays platform-specific file selection window
        /// </summary>
        public static IFileDialog FileDialog(this IServiceContainer sc) => sc.UI().FileDialog;

        /// <summary>
        /// Switches to UI thread asynchonously and then displays the message
        /// </summary>
        public static async Task ShowErrorMessageAsync(this IServiceContainer sc, string message, CancellationToken cancellationToken = default(CancellationToken)) {
            await sc.MainThread().SwitchToAsync(cancellationToken);
            sc.UI().ShowErrorMessage(message);
        }

        /// <summary>
        /// Displays error message in a host-specific UI
        /// </summary>
        public static void ShowErrorMessage(this IServiceContainer sc, string message)
            => sc.UI().ShowErrorMessage(message);

        /// <summary>
        /// Shows the context menu with the specified command ID at the specified location
        /// </summary>
        public static void ShowContextMenu(this IServiceContainer sc, CommandId commandId, int x, int y, object commandTarget = null)
            => sc.UI().ShowContextMenu(commandId, x, y, commandTarget);

        /// <summary>
        /// Displays message with specified buttons in a host-specific UI
        /// </summary>
        public static MessageButtons ShowMessage(this IServiceContainer sc, string message, MessageButtons buttons, MessageType messageType = MessageType.Information)
            => sc.UI().ShowMessage(message, buttons, messageType);

        [Conditional("TRACE")]
        public static void Assert(this IMainThread mainThread, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0) {
            if (mainThread.ThreadId != Thread.CurrentThread.ManagedThreadId) {
                Debug.Fail(FormattableString.Invariant($"{memberName} at {sourceFilePath}:{sourceLineNumber} was incorrectly called from a background thread."));
            }
        }
    }
}
