using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser;

namespace Microsoft.Html.Core.Tree.Events
{
    [ExcludeFromCodeCoverage]
    public class HtmlDocTypeChangeEventArgs : EventArgs
    {
        public DocType DocType { get; private set; }

        public HtmlDocTypeChangeEventArgs(DocType docType)
        {
            DocType = docType;
        }
    }
}
