// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Commands {
    /// <summary>
    /// Main R editor command controller
    /// </summary>
    public class RMainController : ViewController {
        public RMainController(ITextView textView, ITextBuffer textBuffer, IServiceContainer services)
            : base(textView, textBuffer, services) {
            textView.AddService(this);
        }

        /// <summary>
        /// Attaches command controller to the view and projected buffer
        /// </summary>
        public static RMainController Attach(ITextView textView, ITextBuffer textBuffer, IServiceContainer services
            ) {
            var controller = FromTextView(textView);
            return controller ?? new RMainController(textView, textBuffer, services);
        }

        /// <summary>
        /// Retrieves R command controller from text view
        /// </summary>
        public new static RMainController FromTextView(ITextView textView) => textView.GetService<RMainController>();

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
        private bool IsCompletionCommand(Guid group, int id) => Find(group, id) is RCompletionCommandHandler;

        /// <summary>
        /// Disposes main controller and removes it from service manager.
        /// </summary>
        protected override void Dispose(bool disposing) {
            TextView?.RemoveService(this);
            base.Dispose(disposing);
        }
    }
}