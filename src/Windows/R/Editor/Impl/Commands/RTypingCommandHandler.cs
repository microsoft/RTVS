// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Editor.Completions;
using Microsoft.R.Editor.Formatting;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Commands {
    /// <summary>
    /// Processes typing in the R editor document. 
    /// Implements <seealso cref="ICommandTarget" /> 
    /// to receive typing as commands
    /// </summary>
    internal class RTypingCommandHandler : TypingCommandHandler {
        private readonly IServiceContainer _services;
        private readonly AutoFormat _autoFormat;

        public RTypingCommandHandler(ITextView textView, IServiceContainer services)
            : base(textView) {
            _services = services;
            _autoFormat = new AutoFormat(textView, services);
        }

        #region ICommand

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (group == VSConstants.VSStd2K) {
                var typedChar = GetTypedChar(group, id, inputArg);
                if (_autoFormat.IsPreProcessAutoformatTriggerCharacter(typedChar)) {
                    _autoFormat.HandleAutoformat(typedChar);
                }
            }
            return base.Invoke(group, id, inputArg, ref outputArg);
        }

        public override void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg) {
            if (group == VSConstants.VSStd2K) {
                var typedChar = GetTypedChar(group, id, inputArg);
                if (_autoFormat.IsPostProcessAutoformatTriggerCharacter(typedChar)) {
                    _autoFormat.HandleAutoformat(typedChar);
                }

                base.PostProcessInvoke(result, group, id, inputArg, ref outputArg);
            }
        }
        #endregion

        protected override CompletionController CompletionController 
            => CompletionController.FromTextView<RCompletionController>(TextView);
    }
}
