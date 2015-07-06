using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Text
{
    [Export(typeof(ITextViewListener))]
    [Name("DefaultTextViewListener")]
    [ContentType("text")]
    public class TextViewListenerEvents : ITextViewListener
    {
        public static event EventHandler<TextViewListenerEventArgs> TextViewConnected;
        public static event EventHandler<TextViewListenerEventArgs> TextViewDisconnected;

        public TextViewListenerEvents()
        {
        }

        public void OnTextViewConnected(ITextView textView, ITextBuffer textBuffer)
        {
            if (TextViewConnected != null)
            {
                TextViewConnected(this, new TextViewListenerEventArgs(textView, textBuffer));
            }
        }

        public void OnTextViewDisconnected(ITextView textView, ITextBuffer textBuffer)
        {
            if (TextViewDisconnected != null)
            {
                TextViewDisconnected(this, new TextViewListenerEventArgs(textView, textBuffer));
            }
        }
    }
}
