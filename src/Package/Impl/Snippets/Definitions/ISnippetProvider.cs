using System.Collections.Generic;
using System.Xml;

namespace Microsoft.VisualStudio.R.Package.Snippets.Definitions
{
    public interface ISnippetProvider
    {
        IEnumerable<XmlDocument> GetSnippetDocuments(string contentType);
    }
}