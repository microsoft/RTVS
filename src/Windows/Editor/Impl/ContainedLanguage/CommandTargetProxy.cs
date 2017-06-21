// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Text;
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

        public static CommandTargetProxy GetProxyTarget(ITextView textView) {
            var proxy = textView.GetService<CommandTargetProxy>();
            proxy = proxy ?? new CommandTargetProxy(textView);
            return proxy;
        }

        private CommandTargetProxy(ITextView textView) => textView.AddService(this);

        #region ICommandTarget
        public CommandStatus Status(Guid group, int id) 
            => _commandTarget?.Status(@group, id) ?? CommandStatus.NotSupported;

        public CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) => 
            _commandTarget?.Invoke(@group, id, inputArg, ref outputArg) ?? CommandResult.NotSupported;

        public void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg) {
            _commandTarget?.PostProcessInvoke(result, group, id, inputArg, ref outputArg);
        }
        #endregion

        public static void SetCommandTarget(ITextView textView, ICommandTarget target) {
            var proxy = textView.GetService<CommandTargetProxy>();
            if (proxy != null) {
                proxy._commandTarget = target;
                textView.RemoveService(proxy);
            }
        }
    }
}
