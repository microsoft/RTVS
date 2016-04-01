// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.Completion;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Components.Controller;
using Microsoft.R.Editor.Completion;
using Microsoft.R.Editor.Formatting;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Commands {
    /// <summary>
    /// Processes typing in the R editor document. 
    /// Implements <seealso cref="ICommandTarget" /> 
    /// to receive typing as commands
    /// </summary>
    internal class RTypingCommandHandler : TypingCommandHandler {

        public RTypingCommandHandler(ITextView textView)
            : base(textView) {
        }

        #region ICommand

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (group == VSConstants.VSStd2K) {
                char typedChar = GetTypedChar(group, id, inputArg);
                if (AutoFormat.IsPreProcessAutoformatTriggerCharacter(typedChar)) {
                    AutoFormat.HandleAutoformat(TextView, typedChar);
                }
            }
            return base.Invoke(group, id, inputArg, ref outputArg);
        }

        public override void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg) {
            if (group == VSConstants.VSStd2K) {
                char typedChar = GetTypedChar(group, id, inputArg);
                if (AutoFormat.IsPostProcessAutoformatTriggerCharacter(typedChar)) {
                    AutoFormat.HandleAutoformat(TextView, typedChar);
                }

                base.PostProcessInvoke(result, group, id, inputArg, ref outputArg);
            }
        }
        #endregion

        protected override CompletionController CompletionController {
            get { return ServiceManager.GetService<RCompletionController>(TextView); }
        }

    }
}
