using Microsoft.Languages.Editor.Outline;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Outline
{
    internal class ROutlineRegion : OutlineRegion
    {
        private string _displayText;

        public ROutlineRegion(ITextBuffer textBuffer, IAstNode node)
            : base(textBuffer, node)
        {
        }

        public override string DisplayText
        {
            get
            {
                if (_displayText == null)
                {
                    _displayText = _textBuffer.CurrentSnapshot.GetText(this.Start, this.Length);
                    int index = _displayText.IndexOfAny(new char[] { '(', '{' });
                    if(index >= 0)
                    {
                        _displayText = _displayText.Substring(0, index).Trim() + "...";
                    }
                    else if(_displayText.Length > 50)
                    {
                        _displayText = _displayText.Substring(0, 50) + "...";
                    }
                }

                return _displayText;
            }
        }
    }
}
