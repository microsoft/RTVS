using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Text {
    [Export(typeof(ITextBufferListener))]
    [Name(nameof(TextBufferListenerEvents))]
    [ContentType("text")]
    public class TextBufferListenerEvents : ITextBufferListener {
        public static event EventHandler<TextBufferListenerEventArgs> TextBufferCreated;
        public static event EventHandler<TextBufferListenerEventArgs> TextBufferDisposed;

        public void OnTextBufferCreated(ITextBuffer textBuffer) {
            TextBufferCreated?.Invoke(this, new TextBufferListenerEventArgs(textBuffer));
        }

        public void OnTextBufferDisposed(ITextBuffer textBuffer) {
            TextBufferDisposed?.Invoke(this, new TextBufferListenerEventArgs(textBuffer));
        }
    }
}
