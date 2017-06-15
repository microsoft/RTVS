// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Commands.Markdown;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Packages.Markdown;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Commands.MD {
    [Export(typeof(ICommandFactory))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal class VsMdCommandFactory : ICommandFactory {
        private readonly IServiceContainer _services;
        private readonly IRInteractiveWorkflowVisualProvider _workflowProvider;

        [ImportingConstructor]
        public VsMdCommandFactory(ICoreShell coreShell) {
            _services = coreShell.Services;
            _workflowProvider = _services.GetService<IRInteractiveWorkflowVisualProvider>();
        }

        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer) {
            var workflow = _workflowProvider.GetOrCreate();

            if (workflow.ActiveWindow == null) {
                workflow.GetOrCreateVisualComponentAsync()
                    .ContinueOnRanToCompletion(w => w.Container.Show(focus: false, immediate: false));
            }

            return new ICommand[] {
                new ClearReplCommand(textView, workflow),
                new ShowContextMenuCommand(textView, MdGuidList.MdPackageGuid, MdGuidList.MdCmdSetGuid, (int) MarkdownContextMenuId.MD, _services)
            };
        }
    }
}
