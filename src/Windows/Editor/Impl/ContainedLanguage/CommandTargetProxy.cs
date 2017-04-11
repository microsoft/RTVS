// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Services;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.ContainedLanguage {
    /// <summary>
    /// A proxy class that allows delayed chaining of controllers in case
    /// contained language sets its command target first before primary view
    /// is created. When finally the primary view is created it sets actual
    /// command target on the proxy.
    /// </summary>
    public sealed class CommandTargetProxy : ICommandTarget {
        private ICommandTarget _commandTarget;

        public static CommandTargetProxy GetProxyTarget(ITextView textView, ICoreShell coreShell) {
            var proxy = ServiceManager.GetService<CommandTargetProxy>(textView);
            if (proxy == null) {
                proxy = new CommandTargetProxy(textView, coreShell);
            }
            return proxy;
        }

        private CommandTargetProxy(ITextView textView, ICoreShell coreShell) {
            ServiceManager.AddService(this, textView, coreShell);
        }

        #region ICommandTarget
        public CommandStatus Status(Guid group, int id) {
            return _commandTarget != null ? _commandTarget.Status(group, id) : CommandStatus.NotSupported;
        }

        public CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            return _commandTarget != null ? _commandTarget.Invoke(group, id, inputArg, ref outputArg) : CommandResult.NotSupported;
        }

        public void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg) {
            _commandTarget?.PostProcessInvoke(result, group, id, inputArg, ref outputArg);
        }

        #endregion

        public static void SetCommandTarget(ITextView textView, ICommandTarget target) {
            var proxy = ServiceManager.GetService<CommandTargetProxy>(textView);
            if (proxy != null) {
                proxy._commandTarget = target;
                ServiceManager.RemoveService<CommandTargetProxy>(textView);
            }
        }
    }
}
