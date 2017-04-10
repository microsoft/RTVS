// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Commands {
    /// <summary>
    /// Main R editor command controller
    /// </summary>
    public class RMainController : ViewController {
        public RMainController(ITextView textView, ITextBuffer textBuffer, ICoreShell shell)
            : base(textView, textBuffer, shell) {
            ServiceManager.AddService(this, textView, shell);
        }

        /// <summary>
        /// Attaches command controller to the view and projected buffer
        /// </summary>
        public static RMainController Attach(ITextView textView, ITextBuffer textBuffer, ICoreShell shell) {
            RMainController controller = FromTextView(textView);
            if (controller == null) {
                controller = new RMainController(textView, textBuffer, shell);
            }

            return controller;
        }

        /// <summary>
        /// Retrieves R command controller from text view
        /// </summary>
        public static new RMainController FromTextView(ITextView textView) {
            return ServiceManager.GetService<RMainController>(textView);
        }

        public override CommandStatus Status(Guid group, int id) {
            var status = NonRoutedStatus(@group, id, null);
            if ((status & CommandStatus.SupportedAndEnabled) == CommandStatus.SupportedAndEnabled && !IsCompletionCommand(group, id)) {
                return status;
            }

            return base.Status(group, id);
        }

        /// <summary>
        /// Determines if command is one of the completion commands
        /// </summary>
        private bool IsCompletionCommand(Guid group, int id) {
            ICommand cmd = Find(group, id);
            return cmd is RCompletionCommandHandler;
        }

        /// <summary>
        /// Disposes main controller and removes it from service manager.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (TextView != null) {
                ServiceManager.RemoveService<RMainController>(TextView);
            }

            base.Dispose(disposing);
        }
    }
}