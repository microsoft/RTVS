// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Browsers;
using Microsoft.VisualStudio.R.Package.Commands.Markdown;
using Microsoft.VisualStudio.R.Package.Publishing.Commands;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Packages.Markdown;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Commands.MD {
    [Export(typeof(ICommandFactory))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal class VsMdCommandFactory : ICommandFactory {
        private readonly ICoreShell _coreShell;
        private readonly IRInteractiveWorkflowProvider _workflowProvider;
        private readonly IWebBrowserServices _wbs;

        [ImportingConstructor]
        public VsMdCommandFactory(ICoreShell coreShell) {
            _coreShell = coreShell;
            _workflowProvider = coreShell.GetService<IRInteractiveWorkflowProvider>();
            _wbs = coreShell.GetService<IWebBrowserServices>();
        }

        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer) {
            var workflow = _workflowProvider.GetOrCreate();

            if (workflow.ActiveWindow == null) {
                workflow.GetOrCreateVisualComponentAsync()
                    .ContinueOnRanToCompletion(w => w.Container.Show(focus: false, immediate: false));
            }

            var services = workflow.Shell.Services;
            return new ICommand[] {
                new PreviewHtmlCommand(textView, _workflowProvider, services),
                new PreviewPdfCommand(textView, _workflowProvider, services),
                new PreviewWordCommand(textView, _workflowProvider, services),
                new ClearReplCommand(textView, workflow),
                new ShowContextMenuCommand(textView, MdGuidList.MdPackageGuid, MdGuidList.MdCmdSetGuid, (int) MarkdownContextMenuId.MD, _coreShell.Services)
            };
        }
    }
}
