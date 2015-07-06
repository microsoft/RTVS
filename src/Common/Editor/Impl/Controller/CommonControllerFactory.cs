using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Controller
{
    [Export(typeof(IControllerFactory))]
    [ContentType("text")]
    [Name("Default")]
    [Order]
    internal class CommonControllerFactory : IControllerFactory
    {
        public IEnumerable<ICommandTarget> GetControllers(ITextView textView, ITextBuffer textBuffer)
        {
            var list = new List<ICommandTarget>();

            // TODO: activate outlining controller when AST is up
            // list.Add(new OutlineController(textView));
            return list;
        }
    }
}
