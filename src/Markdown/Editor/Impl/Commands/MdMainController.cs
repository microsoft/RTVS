// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Commands {
    /// <summary>
    /// Main R editor command controller
    /// </summary>
    public class MdMainController : ViewController {
        public MdMainController(ITextView textView, ITextBuffer textBuffer)
            : base(textView, textBuffer) {
            ServiceManager.AddService(this, textView);
        }

        public static MdMainController Attach(ITextView textView, ITextBuffer textBuffer) {
            MdMainController controller = FromTextView(textView);
            if (controller == null) {
                controller = new MdMainController(textView, textBuffer);
            }

            return controller;
        }

        public static MdMainController FromTextView(ITextView textView) {
            return ServiceManager.GetService<MdMainController>(textView);
        }

        public override CommandStatus Status(Guid group, int id) {
            if ((NonRoutedStatus(group, id, null) & CommandStatus.SupportedAndEnabled) == CommandStatus.SupportedAndEnabled) {
                return CommandStatus.SupportedAndEnabled;
            }

            var containedCommandTarget = GetContainedCommandTarget();
            if (containedCommandTarget != null) {
                return containedCommandTarget.Status(group, id);
            }
            return base.Status(group, id);
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            // Some commands need to be handled at primary language level (like formatting) rather
            // that routed to contained target. Check if primary controller supports the command
            // and send it there. Command can delegate to contained languages as appropriate.
            if ((NonRoutedStatus(group, id, inputArg) & CommandStatus.SupportedAndEnabled) != CommandStatus.SupportedAndEnabled) {
                var containedCommandTarget = GetContainedCommandTarget();
                if (containedCommandTarget != null) {
                    CommandResult result = containedCommandTarget.Invoke(group, id, inputArg, ref outputArg);
                    if (result.WasExecuted) {
                        return result;
                    }
                }
            }

            return base.Invoke(group, id, inputArg, ref outputArg);
        }

        /// <summary>
        /// Retrieves command target of secondary language if any. Uses current
        /// caret position to detemine if it is in a secondary language block.
        /// </summary>
        public ICommandTarget GetContainedCommandTarget() {
            var containedLanguageHandler = ServiceManager.GetService<IContainedLanguageHandler>(TextBuffer);
            return containedLanguageHandler?.GetCommandTargetOfLocation(TextView, TextView.Caret.Position.BufferPosition);
        }

        /// <summary>
        /// Disposes main controller and removes it from service manager.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (TextView != null) {
                ServiceManager.RemoveService<MdMainController>(TextView);
            }
            base.Dispose(disposing);
        }
    }
}