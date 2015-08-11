using System;
using System.Runtime.InteropServices;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.R.Package;

namespace Microsoft.VisualStudio.R.Languages
{
    [Guid(GuidList.LanguageServiceGuidString)]
    internal sealed class RLanguageService : BaseLanguageService
    {
        public RLanguageService()
            : base(GuidList.LanguageServiceGuid, 
                   RContentTypeDefinition.LanguageName, 
                   RContentTypeDefinition.FileExtension)
        {
        }

        protected override string SaveAsFilter
        {
            get
            {
                return Resources.SaveAsFilterR;
            }
        }
    }
}
