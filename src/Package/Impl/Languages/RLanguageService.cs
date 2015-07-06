using System;
using System.Runtime.InteropServices;
using Microsoft.Languages.Editor.ContentType;
using Microsoft.VisualStudio.R.Package;

namespace Microsoft.VisualStudio.R.Languages
{
    [Guid(RGuidList.LanguageServiceGuidString)]
    internal sealed class RLanguageService : BaseLanguageService
    {
        public RLanguageService()
            : base(RGuidList.LanguageServiceGuid, 
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
