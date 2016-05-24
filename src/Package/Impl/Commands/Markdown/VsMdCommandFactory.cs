// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Commands.Markdown;
using Microsoft.VisualStudio.R.Package.Publishing.Commands;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.Markdown;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Commands.MD {
    [Export(typeof(ICommandFactory))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal class VsMdCommandFactory : ICommandFactory {
        private readonly IRInteractiveWorkflowProvider _workflowProvider;
        private readonly IInteractiveWindowComponentContainerFactory _componentContainerFactory;

        [ImportingConstructor]
        public VsMdCommandFactory(IRInteractiveWorkflowProvider workflowProvider, IInteractiveWindowComponentContainerFactory componentContainerFactory) {
            _workflowProvider = workflowProvider;
            _componentContainerFactory = componentContainerFactory;
        }

        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer) {
            var workflow = _workflowProvider.GetOrCreate();

            if (workflow.ActiveWindow == null) {
                workflow
                    .GetOrCreateVisualComponent(_componentContainerFactory)
                    .ContinueOnRanToCompletion(w => w.Container.Show(false));
            }

            return new ICommand[] {
                new PreviewHtmlCommand(textView, _workflowProvider),
                new PreviewPdfCommand(textView, _workflowProvider),
                new PreviewWordCommand(textView, _workflowProvider),
                new ClearReplCommand(textView, workflow),
                new ShowContextMenuCommand(textView, MdGuidList.MdPackageGuid, MdGuidList.MdCmdSetGuid, (int) MarkdownContextMenuId.MD)
            };
        }
    }
}
