// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Text;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Controllers.Commands;

namespace Microsoft.Languages.Editor.Controllers {
    public abstract class ViewController : Controller {
        private readonly List<ICommandTarget> _controllers;

        /// <summary>
        /// Text view associated with the view controller
        /// </summary>
        public ITextView TextView { get; private set; }
        public ITextBuffer TextBuffer { get; private set; }
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

            foreach (ICommandTarget controller in _controllers) {
                var disposable = controller as IDisposable;
                disposable?.Dispose();
            }

            if (TextView != null) {
                TextViewListenerEvents.TextViewDisconnected -= OnTextViewDisconnected;
            }

            TextView = null;
            TextBuffer = null;
            _controllers.Clear();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public virtual void BuildCommandSet() {
            // It is allowed here not to have host. The reason is that we allow using controller classes
            // without host as long as derived controller is adding commands manually. Without host there is
            // no composition service and hence we are unable to import command factories.
            var cs = Services.GetService<ICompositionService>();
            if (cs != null) {
                var importComposer = new ContentTypeImportComposer<ICommandFactory>(cs);
                var commandFactories = importComposer.GetAll(TextBuffer.ContentType.TypeName);

                foreach (var factory in commandFactories) {
                    var commands = factory.GetCommands(TextView, TextBuffer);
                    AddCommandSet(commands);
                }
            }
        }

        private void BuildControllerSet() {
            var cs = Services.GetService<ICompositionService>();
            var controllerFactories = ComponentLocatorForOrderedContentType<IControllerFactory>.ImportMany(cs, TextBuffer.ContentType);
            if (controllerFactories != null) {
                foreach (var factory in controllerFactories) {
                    _controllers.AddRange(factory.Value.GetControllers(TextView, TextBuffer));
                }
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