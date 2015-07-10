using Microsoft.Languages.Editor.Outline;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Outline
{
    internal class ROutlineRegion : OutlineRegion
    {
        //private string _displayText;
        private EditorDocument _document;

        public ROutlineRegion(ITextBuffer textBuffer, IAstNode node)
            : base(textBuffer, node)
        {
            _document = EditorDocument.FromTextBuffer(textBuffer);
        }
   }
}
