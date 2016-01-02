using System.ComponentModel.Composition;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Completion.Providers {
    /// <summary>
    /// Completion source for Visual Studio core editor
    /// </summary>
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Completion Source Provider")]
    [Order(Before = "default")]
    internal class RCompletionSourceProvider : ICompletionSourceProvider {
        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer) {
            return new RCompletionSource(textBuffer);
        }
    }
}
