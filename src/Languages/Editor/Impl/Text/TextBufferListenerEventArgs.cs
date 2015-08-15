using System;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Text
{
    /// <summary>
    /// event arguments containing a text buffer
    /// </summary>
    public class TextBufferListenerEventArgs : EventArgs
    {
        public ITextBuffer TextBuffer { get; private set; }

        public TextBufferListenerEventArgs(ITextBuffer textBuffer)
        {
            TextBuffer = textBuffer;
        }
    }
}
