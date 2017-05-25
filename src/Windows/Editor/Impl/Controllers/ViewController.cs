// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.Languages.Editor.Text;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.Languages.Editor.Services;

namespace Microsoft.Languages.Editor.Controllers {
    public abstract class ViewController : Controller {
        private readonly List<ICommandTarget> _controllers;

        /// <summary>
        /// Text view associated with the view controller
        /// </summary>
        public ITextView TextView { get; }
        public ITextBuffer TextBuffer { get; }
        protected IServiceContainer Services { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected ViewController(ITextView textView, ITextBuffer textBuffer, IServiceContainer services) {
            TextView = textView;
            TextBuffer = textBuffer;
            Services = services;
            _controllers = new List<ICommandTarget>();

            BuildCommandSet();
            BuildControllerSet();

            TextViewListenerEvents.TextViewDisconnected += OnTextViewDisconnected;
        }

        public static ViewController FromTextView(ITextView textView) => textView.GetService<ViewController>();

        private void OnTextViewDisconnected(object sender, TextViewListenerEventArgs e) {
            if (e.TextView == TextView && e.TextBuffer == TextBuffer) {
                Dispose();
            }
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            foreach (var controller in _controllers) {
                var disposable = controller as IDisposable;
                disposable?.Dispose();
            }

            if (TextView != null) {
                TextViewListenerEvents.TextViewDisconnected -= OnTextViewDisconnected;
            }

            _controllers.Clear();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public virtual void BuildCommandSet() {
            var locator = Services.GetService<IContentTypeServiceLocator>();
            var commandFactories = locator.GetAllServices<ICommandFactory>(TextBuffer.ContentType.TypeName);
            foreach (var factory in commandFactories) {
                var commands = factory.GetCommands(TextView, TextBuffer);
                AddCommandSet(commands);
            }
        }

        private void BuildControllerSet() {
            var locator = Services.GetService<IContentTypeServiceLocator>();
            var controllerFactories = locator.GetAllOrderedServices<IControllerFactory>(TextBuffer.ContentType.TypeName);
            foreach (var factory in controllerFactories) {
                _controllers.AddRange(factory.Value.GetControllers(TextView, TextBuffer));
            }
        }

        public override CommandStatus Status(Guid group, int id) {
            foreach (var controller in _controllers) {
                var status = controller.Status(group, id);
                if (status != CommandStatus.NotSupported) {
                    return status;
                }
            }
            return base.Status(group, id);
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            foreach (var controller in _controllers) {
                var status = controller.Status(group, id);
                if ((status & CommandStatus.SupportedAndEnabled) == CommandStatus.SupportedAndEnabled) {
                    var result = controller.Invoke(group, id, inputArg, ref outputArg);
                    if (result.Status == CommandResult.Executed.Status && result.Result == CommandResult.Executed.Result) {
                        return result;
                    }
                }
            }

            return base.Invoke(group, id, inputArg, ref outputArg);
        }
    }
}