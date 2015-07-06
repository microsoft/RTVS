using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.EditorHelpers;
using Microsoft.Languages.Editor.Shell;

namespace Microsoft.VisualStudio.R.Package.Utilities
{
    public static class TextBufferUtilities
    {
        private static IVsEditorAdaptersFactoryService _adaptersFactoryService;
        private static IVsEditorAdaptersFactoryService AdaptersFactoryService
        {
            get
            {
                if (_adaptersFactoryService == null)
                    _adaptersFactoryService = EditorShell.ExportProvider.GetExport<IVsEditorAdaptersFactoryService>().Value;

                return _adaptersFactoryService;
            }
        }

        public static T QueryInterface<T>(this ITextBuffer textBuffer) where T : class
        {
            var vsTextBuffer = AdaptersFactoryService.GetBufferAdapter(textBuffer);
            return vsTextBuffer as T;
        }

        public static ITextBuffer ToITextBuffer(this IVsTextBuffer vsTextBuffer)
        {
            return AdaptersFactoryService.GetDocumentBuffer(vsTextBuffer);
        }

        public static ITextBuffer ToITextBuffer(this IVsTextLayer vsTextLayer)
        {
            IVsTextLines vsTextLines;
            vsTextLayer.GetBaseBuffer(out vsTextLines);

            return vsTextLines.ToITextBuffer();
        }

        public static ITextBuffer ToITextBuffer(this IVsTextLines vsTextLines)
        {
            return ToITextBuffer(vsTextLines as IVsTextBuffer);
        }

        public static bool ToTextSpan(this ITextRange textRange, ITextBuffer textBuffer, TextSpan[] textSpan)
        {
            bool result = false;

            int line, column;
            if (textBuffer.GetLineColumnFromPosition(textRange.Start, out line, out column))
            {
                textSpan[0].iStartLine = line;
                textSpan[0].iStartIndex = column;

                if (textBuffer.GetLineColumnFromPosition(textRange.End, out line, out column))
                {
                    textSpan[0].iEndLine = line;
                    textSpan[0].iEndIndex = column;

                    result = true;
                }
            }

            return result;
        }
    }
}
