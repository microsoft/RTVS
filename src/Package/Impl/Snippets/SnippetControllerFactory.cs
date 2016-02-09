using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Controller;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Snippets
{
    [Export(typeof(IControllerFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Snippets Command Controller")]
    [Order(Before = "Default")]
    internal class SnippetControllerFactory : IControllerFactory
    {
        public IEnumerable<ICommandTarget> GetControllers(ITextView textView, ITextBuffer textBuffer)
        {
            var list = new List<ICommandTarget>();

            list.Add(new SnippetController(textView, textBuffer));
            return list;
        }
    }
}
