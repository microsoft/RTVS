// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Preview.Commands {
    internal sealed class RunCurrentChunkCommand : RunChunkCommandBase {
        public RunCurrentChunkCommand(ITextView textView, IServiceContainer services) :
            base(textView, services, MdPackageCommandId.icmdRunCurrentChunk) { }

        protected override Task ExecuteAsync() 
            => TextView.GetService<IMarkdownPreview>()?.RunCurrentChunkAsync() ?? Task.CompletedTask;
    }
}
