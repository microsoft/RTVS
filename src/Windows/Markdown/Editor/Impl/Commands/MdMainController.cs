// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Controllers;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.ContainedLanguage;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Commands {
    /// <summary>
    /// Main R editor command controller
    /// </summary>
    public class MdMainController : ViewController {
        private BraceCompletionWorkaround223902 _workaround;

        public MdMainController(ITextView textView, ITextBuffer textBuffer, IServiceContainer services)
            : base(textView, textBuffer, services) {
            textView.AddService(this);
        }

        public static MdMainController Attach(ITextView textView, ITextBuffer textBuffer, IServiceContainer services) {
            var controller = FromTextView(textView);
            return controller ?? new MdMainController(textView, textBuffer, services);
        }

        public new static MdMainController FromTextView(ITextView textView) => textView.GetService<MdMainController>();

        public override ICommandTarget ChainedController {
            get => base.ChainedController;
            set {
                base.ChainedController = value;
                CommandTargetProxy.SetCommandTarget(TextView, value);
            }
        }

        public override CommandStatus Status(Guid group, int id) {
            var status = NonRoutedStatus(@group, id, null);
            if ((status & CommandStatus.SupportedAndEnabled) == CommandStatus.SupportedAndEnabled) {
                return status;
            }

            var containedCommandTarget = GetContainedCommandTarget();
            if (containedCommandTarget != null) {
                return containedCommandTarget.Status(group, id);
            }
            return base.Status(group, id);
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var containedCommandTarget = GetContainedCommandTarget();
            if (containedCommandTarget != null) {
                _workaround = _workaround ?? new BraceCompletionWorkaround223902(TextView);
                var result = containedCommandTarget.Invoke(group, id, inputArg, ref outputArg);
                if (result.WasExecuted) {
                    return result;
                }
            }
            _workaround?.Dispose();
            _workaround = null;

            return base.Invoke(group, id, inputArg, ref outputArg);
        }

        /// <summary>
        /// Retrieves command target of secondary language if any. Uses current
        /// caret position to detemine if it is in a secondary language block.
        /// </summary>
        public ICommandTarget GetContainedCommandTarget() {
            var containedLanguageHandler = TextBuffer.GetService<IContainedLanguageHandler>();
            return containedLanguageHandler?.GetCommandTargetOfLocation(TextView, TextView.Caret.Position.BufferPosition);
        }

        /// <summary>
        /// Disposes main controller and removes it from service manager.
        /// </summary>
        protected override void Dispose(bool disposing) {
            TextView?.RemoveService(this);
            base.Dispose(disposing);
        }

        /// <summary>
        /// Workaround for VS 2015 bug 223902: Brace completion is incorrectly disabled 
        /// in projection scenarios.
        /// </summary>
        /// <remarks>
        /// IDE incorrectly fetches option from view. In projected scenarios view belongs 
        /// to the language that may not have brace completion enabled and that disables 
        /// brace completion in the projected language. Example: R inside markdown.
        /// </remarks>
        sealed class BraceCompletionWorkaround223902 : IDisposable {
            private readonly ITextView _textView;
            private readonly bool _optionValue;

            public BraceCompletionWorkaround223902(ITextView textView) {
                _textView = textView;
                _optionValue = _textView.Options.GetOptionValue(DefaultTextViewOptions.BraceCompletionEnabledOptionId);
                if (!_optionValue) {
                    _textView.Options.SetOptionValue(DefaultTextViewOptions.BraceCompletionEnabledOptionId, true);
                }
            }

            public void Dispose() {
                if (!_optionValue) {
                    _textView.Options.SetOptionValue(DefaultTextViewOptions.BraceCompletionEnabledOptionId, false);
                }
            }
        }
    }
}