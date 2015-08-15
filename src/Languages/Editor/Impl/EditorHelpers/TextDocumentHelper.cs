using System.Diagnostics;
using System.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.EditorHelpers
{
    public static class TextDocumentHelper
    {
        public static void RemoveBOM(this ITextDocument document)
        {
            Encoding encoding = document.Encoding;
            byte[] preamble = encoding.GetPreamble();
            if (preamble.Length > 0)
            {
                document.Encoding = new UTF8Encoding(false);
            }
        }
    }
}
