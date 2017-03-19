// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Tasks;
using Microsoft.Common.Core.Telemetry;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Core.UI.Commands;

namespace Microsoft.Common.Core.Shell {
    public static class ShellExtensions {
        public static T GetService<T>(this ICoreShell shell, Type type = null) where T: class  => shell.GetService<T>(type);

        public static IActionLog Log(this ICoreShell shell) => shell.GetService<IActionLog>();
        public static IFileSystem FileSystem(this ICoreShell shell) => shell.GetService<IFileSystem>();
        public static IProcessServices Process(this ICoreShell shell) => shell.GetService<IProcessServices>();
        public static ITelemetryService Telemetry(this ICoreShell shell) => shell.GetService<ITelemetryService>();
        public static ISecurityService Security(this ICoreShell shell) => shell.GetService<ISecurityService>();
        public static ITaskService Tasks(this ICoreShell shell) => shell.GetService<ITaskService>();

        /// <summary>
        /// Displays application-specific modal progress window
        /// </summary>
        public static IProgressDialog ProgressDialog(this ICoreShell shell) => shell.GetService<IProgressDialog>();

        /// <summary>
        /// Displays platform-specific file selection window
        /// </summary>
        public static IFileDialog FileDialog(this ICoreShell shell) => shell.GetService<IFileDialog>();

        /// <summary>
        /// Displays error message in a host-specific UI
        /// </summary>
        public static void ShowErrorMessage(this ICoreShell shell, string message)
            => shell.GetService<IUIServices>().ShowErrorMessage(message);

        /// <summary>
        /// Shows the context menu with the specified command ID at the specified location
        /// </summary>
        public static void ShowContextMenu(this ICoreShell shell, CommandId commandId, int x, int y, object commandTarget = null)
            => shell.GetService<IUIServices>().ShowContextMenu(commandId, x, y, commandTarget);

        /// <summary>
        /// Displays message with specified buttons in a host-specific UI
        /// </summary>
        public static MessageButtons ShowMessage(this ICoreShell shell, string message, MessageButtons buttons, MessageType messageType = MessageType.Information)
            => shell.GetService<IUIServices>().ShowMessage(message, buttons, messageType);
    }
}
