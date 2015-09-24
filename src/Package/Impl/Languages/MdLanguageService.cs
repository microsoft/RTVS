using System;
using System.Runtime.InteropServices;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.R.Package;

namespace Microsoft.VisualStudio.R.Languages
{
    [Guid(GuidList.MdLanguageServiceGuidString)]
    internal sealed class MdLanguageService : BaseLanguageService
    {
        public MdLanguageService()
            : base(GuidList.MdLanguageServiceGuid,
                   MdContentTypeDefinition.LanguageName,
                   MdContentTypeDefinition.FileExtension1 + ";" + MdContentTypeDefinition.FileExtension2)
        {
        }

        protected override string SaveAsFilter
        {
            get { return Resources.SaveAsFilterMD; }
        }
    }
}
